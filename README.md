# MTConfigGenerator
Generate a config file for a MikroTik Router based on user input

### Overview
C# script that asks the user for configuration information and then produces a configuration file that can be imported into a MikroTik router.

### Requirements
The outputted configuration file has only been tested to work with RouterOS 7

### Config Template
The following is the config template used by the script
```
 @$"/interface bridge
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
        add address={privateNetwork} dns-server={dnsServers} gateway={privateIpAddress} netmask=24
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
        set allowed-interface-list=listBridge"
 
