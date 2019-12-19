import { Dictionary } from '@admin/types';
import { RouteProps } from 'react-router';
import React from 'react';
import IndexPage from '@admin/pages/IndexPage';
import PrototypeAdminDashboard from '@admin/pages/prototypes/PrototypeAdminDashboard';
import PrototypeChartTest from '@admin/pages/prototypes/PrototypeChartTest';
import MethodologyCreateNewConfig from '@admin/pages/prototypes/PrototypeMethodologyConfig';
import MethodologyEditPage from '@admin/pages/prototypes/PrototypeMethodologyEdit';
import MethodologyCreateNew from '@admin/pages/prototypes/PrototypeMethodologyPageCreateNew';
import MethodologyCreateNewStatus from '@admin/pages/prototypes/PrototypeMethodologyStatus';
import PublicationAssignMethodology from '@admin/pages/prototypes/PrototypePublicationPageAssignMethodology';
import PublicationConfirmNew from '@admin/pages/prototypes/PrototypePublicationPageConfirmNew';
import PublicationCreateNew from '@admin/pages/prototypes/PrototypePublicationPageCreateNew';
import PublicationEditPage from '@admin/pages/prototypes/PrototypePublicationPageEditAbsence';
import PublicationEditUnresolvedComments from '@admin/pages/prototypes/PrototypePublicationPageEditAbsenceUnresolvedComments';
import PublicationEditNew from '@admin/pages/prototypes/PrototypePublicationPageEditNew';
import PublicationCreateNewAbsenceConfig from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceConfig';
import PublicationCreateNewAbsenceConfigEdit from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceConfigEdit';
import PublicationCreateNewAbsenceData from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceData';
import PublicationCreateNewAbsenceSchedule from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceSchedule';
import PublicationCreateNewAbsenceScheduleEdit from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceScheduleEdit';
import PublicationCreateNewAbsenceStatus from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceStatus';
import PublicationCreateNewAbsenceTable from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceTable';
import PublicationCreateNewAbsenceViewTables from '@admin/pages/prototypes/PrototypePublicationPageNewAbsenceViewTables';
import PublicationReviewPage from '@admin/pages/prototypes/PrototypePublicationPageReviewAbsence';
import ReleaseCreateNew from '@admin/pages/prototypes/PrototypeReleasePageCreateNew';
import PrototypesIndexPage from '@admin/pages/prototypes/PrototypesIndexPage';
import PrototypeTableTool from '@admin/pages/prototypes/PrototypeTableTool';

const prototypeRouteList: Dictionary<RouteProps> = {
  index: {
    path: '/index',
    component: IndexPage,
  },
  prototypes: {
    path: '/prototypes',
    component: PrototypesIndexPage,
    exact: true,
  },
  prototypeAdminDashboard: {
    path: '/prototypes/admin-dashboard',
    component: PrototypeAdminDashboard,
    exact: true,
  },
  prototypeCharts: {
    path: '/prototypes/charts',
    component: PrototypeChartTest,
    exact: true,
  },
  prototypeTableTool: {
    path: '/prototypes/table-tool',
    component: PrototypeTableTool,
    exact: true,
  },
  prototypePublicationEdit: {
    path: '/prototypes/publication-edit',
    component: PublicationEditPage,
    exact: true,
  },
  prototypeMethodologyEdit: {
    path: '/prototypes/methodology-edit',
    component: MethodologyEditPage,
    exact: true,
  },
  prototypePublicationUnresolvedComments: {
    path: '/prototypes/publication-unresolved-comments',
    component: PublicationEditUnresolvedComments,
    exact: true,
  },
  prototypesPublicationUnresolvedCommentsReview: {
    path: '/prototypes/publication-review',
    render: function UnresolvedCommentsReview() {
      return <PublicationEditUnresolvedComments reviewing />;
    },
  },
  prototypesPublicationHigherReview: {
    path: '/prototypes/publication-higher-review',
    component: PublicationReviewPage,
    exact: true,
  },
  prototypesPublicationReview: {
    path: '/prototypes/publication-preview',
    component: PublicationReviewPage,
    exact: true,
  },
  prototypesPublicationCreateNew: {
    path: '/prototypes/publication-create-new',
    component: PublicationCreateNew,
    exact: true,
  },
  prototypePublicationAssignMethodology: {
    path: '/prototypes/publication-assign-methodology',
    component: PublicationAssignMethodology,
    exact: true,
  },
  prototypesPublicationConfirmNew: {
    path: '/prototypes/publication-confirm-new',
    component: PublicationConfirmNew,
    exact: true,
  },
  prototypesPublicationEditNew: {
    path: '/prototypes/publication-edit-new',
    component: PublicationEditNew,
    exact: true,
  },
  prototypesReleaseCreateNew: {
    path: '/prototypes/release-create-new',
    component: ReleaseCreateNew,
    exact: true,
  },
  prototypesMethodologyCreateNew: {
    path: '/prototypes/methodology-create-new',
    component: MethodologyCreateNew,
    exact: true,
  },
  prototypesPublicationCreateNewAbsence: {
    path: '/prototypes/publication-create-new-absence',
    render: function publicationEditUnresolvedComments() {
      return <PublicationEditUnresolvedComments newBlankRelease />;
    },
    exact: true,
  },
  prototypePublicationCreateNewAbsenceConfig: {
    path: '/prototypes/publication-create-new-absence-config',
    component: PublicationCreateNewAbsenceConfig,
    exact: true,
  },
  prototypePublicationCreateNewMethodologyConfig: {
    path: '/prototypes/publication-create-new-methodology-config',
    component: MethodologyCreateNewConfig,
    exact: true,
  },
  prototypesPublicationCreateNewAbsenceConfigEdit: {
    path: '/prototypes/publication-create-new-absence-config-edit',
    component: PublicationCreateNewAbsenceConfigEdit,
    exact: true,
  },
  prototypesPublicationCreateNewAbsenceData: {
    path: '/prototypes/publication-create-new-absence-data',
    component: PublicationCreateNewAbsenceData,
    exact: true,
  },
  prototypesPublicationCreateNewAbsenceTable: {
    path: '/prototypes/publication-create-new-absence-table',
    component: PublicationCreateNewAbsenceTable,
    exact: true,
  },
  prototypesPublicationCreateNewAbsenceViewTable: {
    path: '/prototypes/publication-create-new-absence-view-table',
    component: PublicationCreateNewAbsenceViewTables,
    exact: true,
  },
  prototypesPublicationCreateNewAbsenceSchedule: {
    path: '/prototypes/publication-create-new-absence-schedule',
    component: PublicationCreateNewAbsenceSchedule,
    exact: true,
  },
  prototypesPublicationCreateNewAbsenceScheduleEdit: {
    path: '/prototypes/publication-create-new-absence-schedule-edit',
    component: PublicationCreateNewAbsenceScheduleEdit,
    exact: true,
  },
  prototypesPublicationCreateNewAbsenceStatus: {
    path: '/prototypes/publication-create-new-absence-status',
    component: PublicationCreateNewAbsenceStatus,
    exact: true,
  },
  prototypesPublicationCreateNewMethodologyStatus: {
    path: '/prototypes/publication-create-new-methodology-status',
    component: MethodologyCreateNewStatus,
    exact: true,
  },
};

export default prototypeRouteList;
