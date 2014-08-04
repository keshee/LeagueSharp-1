#region UpdateModel

            /// <summary>
            /// Gets received when the model changes.
            /// </summary>
            public static class UpdateModel
            {
                public static byte Header = 0x97;

                public static GamePacket Encoded(Struct packetStruct)
                {
                    var result = new GamePacket(Header);
                    result.WriteInteger(packetStruct.NetworkId);
                    result.WriteInteger(packetStruct.NetworkId & ~0x40000000);
                    result.WriteByte((byte) (packetStruct.BOk ? 1 : 0));
                    result.WriteInteger(packetStruct.SkinId);
                    for (var i = 0; i < 32; i++)
                        if(i < packetStruct.ModelName.Length)
                            result.WriteByte((byte) packetStruct.ModelName[i]);
                        else
                            result.WriteByte(0x00);

                    return result;
                }

                public static Struct Decoded(byte[] data)
                {
                    var packet = new GamePacket(data);
                    var result = new Struct();
                    packet.Position = 1;
                    result.NetworkId = packet.ReadInteger();
                    result.Id = packet.ReadInteger();
                    result.BOk = packet.ReadByte() == 0x01;
                    result.SkinId = packet.ReadInteger();

                    return result;
                }

                public struct Struct
                {
                    public int NetworkId;
                    public int Id;
                    public bool BOk;
                    public int SkinId;
                    public string ModelName;

                    public Struct(int networkId, int skinId, string modelName, bool bOk = true, int id = -1)
                    {
                        NetworkId = networkId;
                        Id = id != -1 ? id : NetworkId & ~0x40000000;
                        BOk = bOk;
                        SkinId = skinId;
                        ModelName = modelName;
                    }
                }
            }
