import AdminDocumentationConfigureCharts from '@admin/pages/documentation/DocumentationConfigureCharts';
import AdminDocumentationCreateNewPublication from '@admin/pages/documentation/DocumentationCreateNewPublication';
import AdminDocumentationCreateNewRelease from '@admin/pages/documentation/DocumentationCreateNewRelease';
import AdminDocumentationContentDesignStandards from '@admin/pages/documentation/DocumentationDesignStandards';
import AdminDocumentationEditRelease from '@admin/pages/documentation/DocumentationEditRelease';
import AdminDocumentationGlossary from '@admin/pages/documentation/DocumentationGlossary';
import AdminDocumentationHome from '@admin/pages/documentation/DocumentationHome';
import AdminDocumentationManageContent from '@admin/pages/documentation/DocumentationManageContent';
import AdminDocumentationManageData from '@admin/pages/documentation/DocumentationManageData';
import AdminDocumentationManageDataBlocks from '@admin/pages/documentation/DocumentationManageDataBlocks';
import AdminDocumentationStyle from '@admin/pages/documentation/DocumentationStyle';
import AdminDocumentationUsingDashboard from '@admin/pages/documentation/DocumentationUsingDashboard';
import {
  documentationConfigureChartsRoute,
  documentationContentStandardsRoute,
  documentationCreatePublicationRoute,
  documentationCreateReleaseRoute,
  documentationEditReleaseRoute,
  documentationGlossaryRoute,
  documentationIndexRoute,
  documentationManageContentRoute,
  documentationManageDataBlockRoute,
  documentationManageDataRoute,
  documentationStyleGuideRoute,
  documentationUsingDashboardRoute,
} from './documentationRoutes';

const documentationRoutes = {
  documentationIndexRoute: {
    ...documentationIndexRoute,
    element: <AdminDocumentationHome />,
  },

  documentationContentStandardsRoute: {
    ...documentationContentStandardsRoute,
    element: <AdminDocumentationContentDesignStandards />,
  },

  documentationGlossaryRoute: {
    ...documentationGlossaryRoute,
    element: <AdminDocumentationGlossary />,
  },

  documentationStyleGuideRoute: {
    ...documentationStyleGuideRoute,
    element: <AdminDocumentationStyle />,
  },

  documentationUsingDashboardRoute: {
    ...documentationUsingDashboardRoute,
    element: <AdminDocumentationUsingDashboard />,
  },

  documentationCreateReleaseRoute: {
    ...documentationCreateReleaseRoute,
    element: <AdminDocumentationCreateNewRelease />,
  },

  documentationCreatePublicationRoute: {
    ...documentationCreatePublicationRoute,
    element: <AdminDocumentationCreateNewPublication />,
  },

  documentationEditReleaseRoute: {
    ...documentationEditReleaseRoute,
    element: <AdminDocumentationEditRelease />,
  },

  documentationManageContentRoute: {
    ...documentationManageContentRoute,
    element: <AdminDocumentationManageContent />,
  },

  documentationManageDataRoute: {
    ...documentationManageDataRoute,
    element: <AdminDocumentationManageData />,
  },

  documentationManageDataBlockRoute: {
    ...documentationManageDataBlockRoute,
    element: <AdminDocumentationManageDataBlocks />,
  },

  documentationConfigureChartsRoute: {
    ...documentationConfigureChartsRoute,
    element: <AdminDocumentationConfigureCharts />,
  },
};

export default documentationRoutes;
