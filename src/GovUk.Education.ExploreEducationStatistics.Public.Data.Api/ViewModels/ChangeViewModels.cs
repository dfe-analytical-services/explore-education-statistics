namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A change to the data set, represented as the state before and after the change.
/// </summary>
public abstract record ChangeViewModel<TChange>
{
    /// <summary>
    /// The current state after the change was made.
    /// If the change is an addition, this will be null.
    /// </summary>
    public TChange? CurrentState { get; init; }

    /// <summary>
    /// The previous state before the change was made.
    /// If the change is a deletion, this will be null.
    /// </summary>
    public TChange? PreviousState { get; init; }

    public abstract bool IsMajor();
}

/// <summary>
/// A change to a filter in a data set.
/// </summary>
public record FilterChangeViewModel : ChangeViewModel<FilterViewModel>
{
    public override bool IsMajor()
    {
        if (CurrentState is not null && PreviousState is not null)
        {
            return CurrentState.Id != PreviousState.Id;
        }

        return CurrentState is null && PreviousState is not null;
    }
}

/// <summary>
/// A change to a filter option in a data set.
/// </summary>
public record FilterOptionChangeViewModel : ChangeViewModel<FilterOptionViewModel>
{
    public override bool IsMajor()
    {
        if (CurrentState is not null && PreviousState is not null)
        {
            return CurrentState.Id != PreviousState.Id;
        }

        return CurrentState is null && PreviousState is not null;
    }
}

/// <summary>
/// A change to a geographic level option in a data set.
/// </summary>
public record GeographicLevelChangeViewModel : ChangeViewModel<GeographicLevelViewModel>
{
    public override bool IsMajor() => CurrentState is null && PreviousState is not null;
}

/// <summary>
/// A change to an indicator in a data set.
/// </summary>
public record IndicatorChangeViewModel : ChangeViewModel<IndicatorViewModel>
{
    public override bool IsMajor()
    {
        if (CurrentState is not null && PreviousState is not null)
        {
            return CurrentState.Id != PreviousState.Id;
        }

        return CurrentState is null && PreviousState is not null;
    }
}

/// <summary>
/// A change to a location group in a data set.
/// </summary>
public record LocationGroupChangeViewModel : ChangeViewModel<LocationGroupViewModel>
{
    public override bool IsMajor()
    {
        if (CurrentState is not null && PreviousState is not null)
        {
            return CurrentState.Level != PreviousState.Level;
        }

        return CurrentState is null && PreviousState is not null;
    }
}

/// <summary>
/// A change to a location option in a data set.
/// </summary>
public record LocationOptionChangeViewModel : ChangeViewModel<LocationOptionViewModel>
{
    public override bool IsMajor()
    {
        if (CurrentState is not null && PreviousState is not null)
        {
            return CurrentState.Id != PreviousState.Id
                || CurrentState.HasMajorChange(PreviousState);
        }

        return CurrentState is null && PreviousState is not null;
    }
}

/// <summary>
/// A change to a time period option in a data set.
/// </summary>
public record TimePeriodOptionChangeViewModel : ChangeViewModel<TimePeriodOptionViewModel>
{
    public override bool IsMajor()
    {
        if (CurrentState is not null && PreviousState is not null)
        {
            return CurrentState.Code != PreviousState.Code
                || CurrentState.Period != PreviousState.Period;
        }

        return CurrentState is null && PreviousState is not null;
    }
}
