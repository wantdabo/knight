﻿using Ape.Volo.Common.Global;
using SqlSugar;

namespace Ape.Volo.Entity.Monitor;

/// <summary>
/// 
/// </summary>
[Tenant(SqlSugarConfig.LogId)]
[SplitTable(SplitType.Month)]
[SugarTable($@"{"log_warning"}_{{year}}{{month}}{{day}}")]
public class WarningLog : SerilogBase
{
}
