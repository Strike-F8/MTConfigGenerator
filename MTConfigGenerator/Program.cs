﻿using System.Net;
using System.Net.Sockets;

class Program
{
    /// <summary>
    /// Checks if the provided IP address is valid. The type parameter can either be "private" or "public"
    /// The method checks if the provided IP address is a valid private or public address according to the passed in type
    /// </summary>
    /// <param name="ipString"></param>
    /// <param name="type"></param>
    /// <returns>True or False</returns>
    public static bool ValidateIPAddress(string ipString, string type)
    {
        // Check if the input is an IP address
        if (!IPAddress.TryParse(ipString, out IPAddress parsedAddress))
        {
            Console.WriteLine($"Please enter a valid {type} IP address");

            return false;
        }

        if (parsedAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // Check if the IP address is within the range of private IP addresses
            byte[] bytes = parsedAddress.GetAddressBytes();
            if (bytes[0] == 10 ||
                (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                (bytes[0] == 192 && bytes[1] == 168))
            {
                if(type == "private")
                    return true;
                else
                {
                    Console.WriteLine($"Please enter a valid {type} IP address");
                    return false;
                }    
            }
        }

        // Check if the IP address is a loopback address
        if (IPAddress.IsLoopback(parsedAddress))
        {
            Console.WriteLine($"Please enter a valid {type} IP address");

            return false;
        }

        // Check if the IP address is a link-local address
        if (parsedAddress.IsIPv6LinkLocal)
        {
            Console.WriteLine($"Please enter a valid {type} IP address");

            return false;
        }

        return true;
    }
    /// <summary>
    /// Calculate the router's private network based on the provided private IP address
    /// </summary>
    /// <param name="ipString"></param>
    /// <returns>Returns a /24 network</returns>
    public static string CalcPrivateNetwork(string ipString)
    {
        string[] address = ipString.Split('.');

        address[address.Length - 1] = "0";

        string result="";

        foreach(string part in address)
            result += part + ".";

        return result[0..(result.Length-1)];
    }

    /// <summary>
    /// Calculate the default DHCP range based on the private subnet
    /// </summary>
    /// <param name="ipString"></param>
    /// <returns></returns>
    public static string CalcDHCPRange(string ipString)
    {
        // Calculate the default DHCP range
        string[] address = ipString.Split(".");
        string result = "";

        for(int i = 0; i < address.Length-1; i++)
            result += address[i] + ".";

        return $"{result}10-{result}250";
    }

    /// <summary>
    /// If the user wants to change the default DHCP range, ask for it here.
    /// </summary>
    /// <param name="ipString"></param>
    /// <returns></returns>
    public static string GetDHCPRange(string ipString)
    {
        // Get a DHCP range from the user
        string[] address = ipString.Split('.');

        string begin = "";
        for (int i = 0; i < address.Length - 1; i++)
            begin += address[i] + ".";

        Console.WriteLine("Please enter the beginning of the DHCP range");
        Console.Write(begin);
        string dhcpStart = Console.ReadLine();

        Console.WriteLine("Please enter the end of the DHCP range\n");
        Console.Write(begin);
        string dhcpEnd = Console.ReadLine();

        return $"{begin+dhcpStart}-{begin+dhcpEnd}";
    }

    static async Task Main()
    {
        // Retrieve user inputs
        Console.WriteLine("Enter the name of the router: (Default: Unnamed Router)");
        string routerName=Console.ReadLine();

        if (routerName.Equals(""))
            routerName = "Unnamed Router";

        string privateIpAddress;
        do
        {
            Console.WriteLine("Enter the private IP address of the router: (Default: 192.168.89.1)");
            privateIpAddress = Console.ReadLine();
            if(privateIpAddress.Equals(""))
                privateIpAddress = "192.168.89.1";
        } while (!ValidateIPAddress(privateIpAddress, "private")); // Loop if the provided address is not valid

        string privateNetwork = CalcPrivateNetwork(privateIpAddress);
       
        //DNS
        String dnsServers = "";
        while(true)
        {
            // Loop until the user provides at least one DNS server
            Console.WriteLine("Enter the DNS servers provided by the ISP (comma-separated or leave blank):");
            dnsServers = Console.ReadLine();

            Console.WriteLine("Would you like to add Google's DNS servers? (8.8.8.8,8.8.4.4)");
            string googleDNS = Console.ReadLine().ToLower();

            if (googleDNS.Equals("y") || googleDNS.Equals("yes") || googleDNS.Equals(""))
            {
                // Only add a comma if the list of addresses is not empty. Else, the list will begin with a comma and cause an error
                if (dnsServers != "")
                    dnsServers += ",";

                Console.WriteLine("Adding Google DNS");
                dnsServers += "8.8.8.8,8.8.4.4";
            }
            else
                Console.WriteLine("Skipping Google DNS");

            Console.WriteLine("Would you like to add OpenDNS's DNS servers? (208.67.222.222,208.67.220.220)");
            string openDNS = Console.ReadLine().ToLower();
            if (openDNS.Equals("y") || openDNS.Equals("yes") || openDNS.Equals(""))
            {
                if (dnsServers != "")
                    dnsServers += ",";

                Console.WriteLine("Adding openDNS");
                dnsServers += "208.67.222.222,208.67.220.220";
            }
            else
                Console.WriteLine("Skipping openDNS");

            // Loop if the user didn't provide any DNS servers
            if (dnsServers != "")
                break;
            else
                Console.WriteLine("Please choose at least one DNS server");
        }

        // DHCP Range
        string dhcpRange = CalcDHCPRange(privateIpAddress);
        Console.WriteLine($"DHCP Range is {dhcpRange}. Is this okay?");
        string response = Console.ReadLine().ToLower();
        if (!(response.Equals("y") || response.Equals("yes") || response.Equals("")))
            dhcpRange = GetDHCPRange(privateIpAddress);

        string configuration=@$"# MikroTik configuration
        /interface bridge
        add name=local
        /interface list
        add name=listBridge
        /interface list member add list=listBridge interface=local
        /ip pool
        add name=dhcp ranges={dhcpRange}
        /ip dhcp-server
        add address-pool=dhcp interface=local lease-time=3d name=dhcp1
        /interface bridge port
        add bridge=local ingress-filtering=no interface=ether2
        add bridge=local ingress-filtering=no interface=ether3
        add bridge=local ingress-filtering=no interface=ether4
        add bridge=local ingress-filtering=no interface=ether5
        add bridge=local ingress-filtering=no interface=ether6
        add bridge=local ingress-filtering=no interface=ether7
        add bridge=local ingress-filtering=no interface=ether8
        add bridge=local ingress-filtering=no interface=ether9
        add bridge=local ingress-filtering=no interface=ether10
        /ip address
        add address={privateIpAddress}/24 interface=local network={privateNetwork}
        /ip dhcp-client
        add interface=ether1
        /ip dhcp-server network
        add address={privateNetwork}/24 dns-server={dnsServers} gateway={privateIpAddress} netmask=24
        /ip firewall filter
        add action=accept chain=input comment=""Allow SIP traffic"" dst-port=5060 in-interface=ether1 protocol=udp
        add action=accept chain=input comment=""accept established,related"" connection-state=established,related
        add action=drop chain=input connection-state=invalid
        add action=accept chain=input comment=""allow ICMP"" in-interface=ether1 protocol=icmp
        add action=accept chain=input comment=""allow SSH"" in-interface=ether1 port=22 protocol=tcp
        add action=drop chain=input comment=""block everything else"" in-interface=ether1
        /ip service
        set telnet disabled=yes
        set ftp disabled=yes
        /system clock
        set time-zone-name=America/New_York
        /system identity
        set name=""{routerName}""
        /tool bandwidth-server
        set enabled=no
        /tool mac-server
        set allowed-interface-list=listBridge
        /tool mac-server mac-winbox
        set allowed-interface-list=listBridge";

        Console.WriteLine(configuration);
        Console.WriteLine($"Write this configuration file to {routerName}.rsc?");
        response = Console.ReadLine().ToLower();
        if(response.Equals("y") || response.Equals("yes") || response.Equals(""))
        {
            Console.WriteLine($"Writing configuration file to {routerName}.rsc");
            File.WriteAllText($"{routerName}.rsc", configuration);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("Configuration not written to file");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}