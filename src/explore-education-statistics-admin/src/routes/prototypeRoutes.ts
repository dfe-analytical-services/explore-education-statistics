import { Dictionary } from '@admin/types';
import { RouteProps } from 'react-router';
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

const prototypeRouteList: Dictionary<RouteProps> = {};

export default prototypeRouteList;

/* Prototype Routes
<Route exact path="/index" component={IndexPage} />

<Route
	exact
	path="/prototypes/"
	component={PrototypesIndexPage}
/>

<Route
	exact
	path="/prototypes/admin-dashboard"
	component={PrototypeAdminDashboard}
/>

<Route
	exact
	path="/prototypes/charts"
	component={PrototypeChartTest}
/>
<Route
	exact
	path="/prototypes/table-tool"
	component={PrototypeTableTool}
/>

<Route
	exact
	path="/prototypes/publication-edit"
	component={PublicationEditPage}
/>
<Route
	exact
	path="/prototypes/methodology-edit"
	component={MethodologyEditPage}
/>
<Route
	exact
	path="/prototypes/publication-unresolved-comments"
	component={PublicationEditUnresolvedComments}
/>
<Route
	exact
	path="/prototypes/publication-review"
	render={() => <PublicationEditUnresolvedComments reviewing />}
/>
<Route
	exact
	path="/prototypes/publication-higher-review"
	component={PublicationReviewPage}
/>
<Route
	exact
	path="/prototypes/publication-preview"
	component={PublicationReviewPage}
	render={() => <PublicationReviewPage />}
/>
<Route
	exact
	path="/prototypes/publication-create-new"
	component={PublicationCreateNew}
/>
<Route
	exact
	path="/prototypes/publication-assign-methodology"
	component={PublicationAssignMethodology}
/>
<Route
	exact
	path="/prototypes/publication-confirm-new"
	component={PublicationConfirmNew}
/>
<Route
	exact
	path="/prototypes/publication-edit-new"
	component={PublicationEditNew}
/>
<Route
	exact
	path="/prototypes/release-create-new"
	component={ReleaseCreateNew}
/>
<Route
	exact
	path="/prototypes/methodology-create-new"
	component={MethodologyCreateNew}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence"
	render={() => (
		<PublicationEditUnresolvedComments newBlankRelease />
	)}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-config"
	component={PublicationCreateNewAbsenceConfig}
/>
<Route
	exact
	path="/prototypes/publication-create-new-methodology-config"
	component={MethodologyCreateNewConfig}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-config-edit"
	component={PublicationCreateNewAbsenceConfigEdit}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-data"
	component={PublicationCreateNewAbsenceData}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-table"
	component={PublicationCreateNewAbsenceTable}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-view-table"
	component={PublicationCreateNewAbsenceViewTables}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-schedule"
	component={PublicationCreateNewAbsenceSchedule}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-schedule-edit"
	component={PublicationCreateNewAbsenceScheduleEdit}
/>
<Route
	exact
	path="/prototypes/publication-create-new-absence-status"
	component={PublicationCreateNewAbsenceStatus}
/>
<Route
	exact
	path="/prototypes/publication-create-new-methodology-status"
	component={MethodologyCreateNewStatus}
/>
 */
