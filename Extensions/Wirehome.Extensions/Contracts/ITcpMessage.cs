﻿namespace Wirehome.Extensions.Contracts
{
    public interface ITcpMessage
    {
        string Address { get; set; }
        string Code { get; set; }
        int Port { get; set; }

        string MessageAddress();
        byte[] Serialize();
    }
}