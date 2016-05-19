module Models

type BuildHistoryModel =
    {
        Branch              : string option
        BuildCount          : int
        IncludePullRequests : bool
        ShowStats           : bool
    }
    static member SetBranch model branch =
        {
            Branch              = branch
            BuildCount          = model.BuildCount
            IncludePullRequests = model.IncludePullRequests
            ShowStats           = model.ShowStats
        }
    static member SetBuildCount model buildCount =
        {
            Branch              = model.Branch
            BuildCount          = buildCount
            IncludePullRequests = model.IncludePullRequests
            ShowStats           = model.ShowStats
        }
    static member SetIncludePullRequests model includePullRequests =
        {
            Branch              = model.Branch
            BuildCount          = model.BuildCount
            IncludePullRequests = includePullRequests
            ShowStats           = model.ShowStats
        }
    static member SetShowStats model showStats =
        {
            Branch              = model.Branch
            BuildCount          = model.BuildCount
            IncludePullRequests = model.IncludePullRequests
            ShowStats           = showStats
        }
    static member Default =
        {
            Branch              = None
            BuildCount          = 25
            IncludePullRequests = true
            ShowStats           = true
        }