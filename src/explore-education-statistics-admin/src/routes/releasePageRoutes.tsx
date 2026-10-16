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
    element: <ReleaseSummaryPage />,
  },
  {
    ...releaseDataRoute,
    element: <ReleaseDataPage />,
  },
  {
    ...releaseFootnotesRoute,
    element: <ReleaseFootnotesPage />,
  },
  {
    ...releaseDataBlocksRoute,
    element: <ReleaseDataBlocksPage />,
  },
  {
    ...releaseContentRoute,
    element: <ReleaseContentPage />,
  },
  {
    ...releaseChecklistRoute,
    element: <ReleasePublishChecklistPage />,
  },
  {
    ...releaseStatusRoute,
    element: <ReleasePublishStatusPage />,
  },
  {
    ...releasePreReleaseAccessRoute,
    element: <ReleasePreReleaseAccessPage />,
  },
  {
    ...releaseAncillaryFilesRoute,
    element: <ReleaseDataPage />,
  },
  {
    ...releaseAncillaryFileRoute,
    element: <ReleaseAncillaryFilePage />,
  },
  {
    ...releaseDataFileReplaceRoute,
    element: <ReleaseDataFileReplacePage />,
  },
  {
    ...releaseDataFileReplacementCompleteRoute,
    element: <ReleaseDataFileReplacementCompletePage />,
  },
  {
    ...releaseApiDataSetsRoute,
    element: <ReleaseDataPage />,
  },
  {
    ...releaseApiDataSetDetailsRoute,
    element: <ReleaseApiDataSetDetailsPage />,
  },
  {
    ...releaseApiDataSetFiltersMappingRoute,
    element: <ReleaseApiDataSetFiltersMappingPage />,
  },
  {
    ...releaseApiDataSetLocationsMappingRoute,
    element: <ReleaseApiDataSetLocationsMappingPage />,
  },
  {
    ...releaseApiDataSetIndicatorsMappingRoute,
    element: <ReleaseApiDataSetIndicatorsMappingPage />,
  },
  {
    ...releaseApiDataSetPreviewRoute,
    element: <ReleaseApiDataSetPreviewPage />,
  },
  {
    ...releaseApiDataSetPreviewTokenRoute,
    element: <ReleaseApiDataSetPreviewTokenPage />,
  },
  {
    ...releaseApiDataSetPreviewTokenLogRoute,
    element: <ReleaseApiDataSetPreviewTokenLogPage />,
  },
  {
    ...releaseApiDataSetVersionHistoryRoute,
    element: <ReleaseApiDataSetVersionHistoryPage />,
  },
  {
    ...releaseApiDataSetChangelogRoute,
    element: <ReleaseApiDataSetChangelogPage />,
  },
  {
    ...releaseSummaryEditRoute,
    element: <ReleaseSummaryEditPage />,
  },
  {
    ...releaseFootnotesCreateRoute,
    element: <ReleaseFootnoteCreatePage />,
  },
  {
    ...releaseFootnotesEditRoute,
    element: <ReleaseFootnoteEditPage />,
  },
  {
    ...releaseTableToolRoute,
    element: <ReleaseTableToolPage />,
  },
  {
    ...releaseDataBlockCreateRoute,
    element: <ReleaseDataBlockCreatePage />,
  },
  {
    ...releaseDataBlockEditRoute,
    element: <ReleaseDataBlockEditPage />,
  },
];

export default releasePageRoutes;
