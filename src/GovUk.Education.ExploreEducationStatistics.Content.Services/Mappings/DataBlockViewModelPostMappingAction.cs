using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;

/// <summary>
/// An AfterMap class that AutoMapper can use to fill in additional details after a source object has been
/// mapped to its destination object.
///
/// In this particular instance, we are using this to transparently add a missing "DataBlockParentId" to any
/// <see cref="DataBlockViewModel" /> destination instances if they have been mapped from a source
/// <see cref="DataBlock"/>, which does not have a DataBlockParentId present in its attributes.
///
/// Note that this is not necessary to do for <see cref="DataBlockViewModel" /> destination instances if the source is
/// <see cref="DataBlockVersion"/>, because this class carries the necessary DataBlockParentId on it. The reason that we
/// don't simply enforce that we always use <see cref="DataBlockVersion"/> as the source rather than
/// <see cref="DataBlock"/> is that when we are mapping an entire Release Content tree (ContentSections and
/// ContentBlocks), we don't have the option here of using DataBlockVersions rather than DataBlocks.
///
/// This will not be necessary to continue to support after DataBlock is removed entirely from the ContentBlock domain
/// model and absorbed into DataBlockVersion, but for now this is the safest way to ensure that we catch every place in
/// the code where we're able to map a DataBlock into a DataBlockViewModel using AutoMapper, no matter the depth.
/// </summary>
public class DataBlockViewModelPostMappingAction : IMappingAction<DataBlock, DataBlockViewModel>
{
    private readonly ContentDbContext _context;

    public DataBlockViewModelPostMappingAction(ContentDbContext context)
    {
        _context = context;
    }

    public void Process(DataBlock source, DataBlockViewModel destination, ResolutionContext context)
    {
        var dataBlockParentId = _context
            .DataBlockVersions
            .First(dataBlockVersion => dataBlockVersion.Id == destination.Id)
            .DataBlockParentId;

        destination.DataBlockParentId = dataBlockParentId;
    }
}
