module Models

type BuildHistoryModel =
    {
        Branch                  : string option
        BuildCount              : int
        IncludeFromPullRequests : bool
        ShowStats               : bool
    }
    static member SetBranch model branch =
        {
            Branch                  = branch
            BuildCount              = model.BuildCount
            IncludeFromPullRequests = model.IncludeFromPullRequests
            ShowStats               = model.ShowStats
        }
    static member SetBuildCount model buildCount =
        {
            Branch                  = model.Branch
            BuildCount              = buildCount
            IncludeFromPullRequests = model.IncludeFromPullRequests
            ShowStats               = model.ShowStats
        }
    static member SetIncludeFromPullRequests model includeFromPullRequests =
        {
            Branch                  = model.Branch
            BuildCount              = model.BuildCount
            IncludeFromPullRequests = includeFromPullRequests
            ShowStats               = model.ShowStats
        }
    static member SetShowStats model showStats =
        {
            Branch                  = model.Branch
            BuildCount              = model.BuildCount
            IncludeFromPullRequests = model.IncludeFromPullRequests
            ShowStats               = showStats
        }
    static member Default =
        {
            Branch                  = None
            BuildCount              = 25
            IncludeFromPullRequests = true
            ShowStats               = true
        }