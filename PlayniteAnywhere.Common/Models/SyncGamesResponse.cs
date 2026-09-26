using System;
using System.Collections.Generic;

public class SyncGamesResponse
{
    public int received { get; set; }
    public int deleted { get; set; }
    public int total { get; set; }
    public List<Guid> coversToSync { get; set; } = new List<Guid>();
}