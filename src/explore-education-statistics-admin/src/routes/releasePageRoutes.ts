import ReleaseContentPage from '@admin/pages/release/content/ReleaseContentPage';
import ReleaseAncillaryFilePage from '@admin/pages/release/data/ReleaseAncillaryFilePage';
import ReleaseApiDataSetChangelogPage from '@admin/pages/release/data/ReleaseApiDataSetChangelogPage';
import ReleaseApiDataSetDetailsPage from '@admin/pages/release/data/ReleaseApiDataSetDetailsPage';
import ReleaseApiDataSetFiltersMappingPage from '@admin/pages/release/data/ReleaseApiDataSetFiltersMappingPage';
import ReleaseApiDataSetIndicatorsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetIndicatorsMappingPage';
import ReleaseApiDataSetLocationsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import ReleaseApiDataSetPreviewPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewPage';
import ReleaseApiDataSetPreviewTokenLogPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenLogPage';
import ReleaseApiDataSetPreviewTokenPage from '@admin/pages/release/data/ReleaseApiDataSetPreviewTokenPage';
import ReleaseApiDataSetVersionHistoryPage from '@admin/pages/release/data/ReleaseApiDataSetVersionHistoryPage';
import ReleaseDataFileReplacePage from '@admin/pages/release/data/ReleaseDataFileReplacePage';
import ReleaseDataFileReplacementCompletePage from '@admin/pages/release/data/ReleaseDataFileReplacementCompletePage';
import ReleaseDataPage from '@admin/pages/release/data/ReleaseDataPage';
import ReleaseDataBlockCreatePage from '@admin/pages/release/datablocks/ReleaseDataBlockCreatePage';
import ReleaseDataBlockEditPage from '@admin/pages/release/datablocks/ReleaseDataBlockEditPage';
import ReleaseDataBlocksPage from '@admin/pages/release/datablocks/ReleaseDataBlocksPage';
import ReleaseTableToolPage from '@admin/pages/release/datablocks/ReleaseTableToolPage';
import ReleaseFootnoteCreatePage from '@admin/pages/release/footnotes/ReleaseFootnoteCreatePage';
import ReleaseFootnoteEditPage from '@admin/pages/release/footnotes/ReleaseFootnoteEditPage';
import ReleaseFootnotesPage from '@admin/pages/release/footnotes/ReleaseFootnotesPage';
import ReleasePreReleaseAccessPage from '@admin/pages/release/pre-release/ReleasePreReleaseAccessPage';
import ReleasePublishChecklistPage from '@admin/pages/release/ReleaseChecklistPage';
import ReleasePublishStatusPage from '@admin/pages/release/ReleaseStatusPage';
import ReleaseSummaryEditPage from '@admin/pages/release/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/ReleaseSummaryPage';
import {
  releaseAncillaryFileRoute,
  releaseAncillaryFilesRoute,
  releaseApiDataSetChangelogRoute,
  releaseApiDataSetDetailsRoute,
  releaseApiDataSetFiltersMappingRoute,
  releaseApiDataSetIndicatorsMappingRoute,
  releaseApiDataSetLocationsMappingRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenLogRoute,
  releaseApiDataSetPreviewTokenRoute,
  releaseApiDataSetsRoute,
  releaseApiDataSetVersionHistoryRoute,
  releaseChecklistRoute,
  releaseContentRoute,
  releaseDataBlockCreateRoute,
  releaseDataBlockEditRoute,
  releaseDataBlocksRoute,
  releaseDataFileReplacementCompleteRoute,
  releaseDataFileReplaceRoute,
  releaseDataRoute,
  releaseFootnotesCreateRoute,
  releaseFootnotesEditRoute,
  releaseFootnotesRoute,
  releasePreReleaseAccessRoute,
  releaseStatusRoute,
  releaseSummaryEditRoute,
  releaseSummaryRoute,
  releaseTableToolRoute,
} from '@admin/routes/releaseRoutes';
import { NavRouteProps } from '@admin/routes/types';

const releasePageRoutes: NavRouteProps[] = [
  {
    ...releaseSummaryRoute,
    component: ReleaseSummaryPage,
  },
  {
    ...releaseDataRoute,
    component: ReleaseDataPage,
  },
  {
    ...releaseFootnotesRoute,
    component: ReleaseFootnotesPage,
  },
  {
    ...releaseDataBlocksRoute,
    component: ReleaseDataBlocksPage,
  },
  {
    ...releaseContentRoute,
    component: ReleaseContentPage,
  },
  {
    ...releaseChecklistRoute,
    component: ReleasePublishChecklistPage,
  },
  {
    ...releaseStatusRoute,
    component: ReleasePublishStatusPage,
  },
  {
    ...releasePreReleaseAccessRoute,
    component: ReleasePreReleaseAccessPage,
  },
  {
    ...releaseAncillaryFilesRoute,
    component: ReleaseDataPage,
  },
  {
    ...releaseAncillaryFileRoute,
    component: ReleaseAncillaryFilePage,
  },
  {
    ...releaseDataFileReplaceRoute,
    component: ReleaseDataFileReplacePage,
  },
  {
    ...releaseDataFileReplacementCompleteRoute,
    component: ReleaseDataFileReplacementCompletePage,
  },
  {
    ...releaseApiDataSetsRoute,
    component: ReleaseDataPage,
  },
  {
    ...releaseApiDataSetDetailsRoute,
    component: ReleaseApiDataSetDetailsPage,
  },
  {
    ...releaseApiDataSetFiltersMappingRoute,
    component: ReleaseApiDataSetFiltersMappingPage,
  },
  {
    ...releaseApiDataSetLocationsMappingRoute,
    component: ReleaseApiDataSetLocationsMappingPage,
  },
  {
    ...releaseApiDataSetIndicatorsMappingRoute,
    component: ReleaseApiDataSetIndicatorsMappingPage,
  },
  {
    ...releaseApiDataSetPreviewRoute,
    component: ReleaseApiDataSetPreviewPage,
  },
  {
    ...releaseApiDataSetPreviewTokenRoute,
    component: ReleaseApiDataSetPreviewTokenPage,
  },
  {
    ...releaseApiDataSetPreviewTokenLogRoute,
    component: ReleaseApiDataSetPreviewTokenLogPage,
  },
  {
    ...releaseApiDataSetVersionHistoryRoute,
    component: ReleaseApiDataSetVersionHistoryPage,
  },
  {
    ...releaseApiDataSetChangelogRoute,
    component: ReleaseApiDataSetChangelogPage,
  },
  {
    ...releaseSummaryEditRoute,
    component: ReleaseSummaryEditPage,
  },
  {
    ...releaseFootnotesCreateRoute,
    component: ReleaseFootnoteCreatePage,
  },
  {
    ...releaseFootnotesEditRoute,
    component: ReleaseFootnoteEditPage,
  },
  {
    ...releaseTableToolRoute,
    component: ReleaseTableToolPage,
  },
  {
    ...releaseDataBlockCreateRoute,
    component: ReleaseDataBlockCreatePage,
  },
  {
    ...releaseDataBlockEditRoute,
    component: ReleaseDataBlockEditPage,
  },
];

export default releasePageRoutes;
