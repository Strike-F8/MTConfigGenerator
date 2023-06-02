class Program
{
    static async Task Main()
    {
        // Retrieve user inputs

        Console.WriteLine("Enter the name of the router");
        string routerName=Console.ReadLine();

        Console.WriteLine("Enter the ISP-provided IP address:");
        string ispIpAddress=Console.ReadLine();

        Console.WriteLine("Enter the private IP address of the router:");
        string privateIpAddress=Console.ReadLine();
        string privateNetwork=privateIpAddress.Substring(0, privateIpAddress.Length-1) + "0";

        String dnsServers = "";
        //DNS
        while(true)
        {
            Console.WriteLine("Enter the DNS servers provided by the ISP (comma-separated or leave blank):");
            dnsServers = Console.ReadLine();

            Console.WriteLine("Would you like to add Google's DNS servers? (8.8.8.8,8.8.4.4)");
            string googleDNS = Console.ReadLine();

            if (googleDNS.ToLower().Equals("y") || googleDNS.ToLower().Equals("yes") || googleDNS == null)
            {
                if (dnsServers != "")
                    dnsServers += ",";

                Console.WriteLine("Adding Google DNS");
                dnsServers += "8.8.8.8,8.8.4.4";
            }
            else
                Console.WriteLine("Skipping Google DNS");

            Console.WriteLine("Would you like to add OpenDNS's DNS servers? (208.67.222.222,208.67.220.220)");
            string openDNS = Console.ReadLine();
            if (openDNS.ToLower().Equals("y") || openDNS.ToLower().Equals("yes") || openDNS == null)
            {
                if (dnsServers != "")
                    dnsServers += ",";

                Console.WriteLine("Adding openDNS");
                dnsServers += "208.67.222.222,208.67.220.220";
            }
            else
                Console.WriteLine("Skipping openDNS");

            if (dnsServers != "")
                break;
            else
                Console.WriteLine("Please choose at least one DNS server");
        }
        

        Console.WriteLine("Enter the DHCP range (start-end):");
        string dhcpRange=Console.ReadLine();

        string configuration=@$"# MikroTik configuration
        /interface bridge
        add name=local
        /interface list
        add name=WAN
        add name=LAN
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
        File.WriteAllText("config.rsc", configuration);
    }
}
