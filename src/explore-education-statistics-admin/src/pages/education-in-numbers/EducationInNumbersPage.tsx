import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import RouteSwitch from '@admin/components/RouteSwitch';
import { EducationInNumbersPageContextProvider } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import educationInNumbersPageRoutes from '@admin/routes/educationInNumbersPageRoutes';
import {
  educationInNumbersNavRoutes,
  EducationInNumbersRouteParams,
} from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersService, {
  EinSummary,
} from '@admin/services/educationInNumbersService';
import useNavRoutes from '@admin/hooks/useNavRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const EducationInNumbersPage = ({
  match,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
  const { educationInNumbersPageId } = match.params;

  const {
    value: educationInNumbersPage,
    setState: setEducationInNumbersPage,
    isLoading,
  } = useAsyncHandledRetry(() => {
    return educationInNumbersService.getEducationInNumbersPage(
      educationInNumbersPageId,
    );
  }, [educationInNumbersPageId]);

  const {
    navBarRoutes,
    currentRouteTitle: pageRouteTitle,
    previousSection,
    nextSection,
  } = useNavRoutes(educationInNumbersNavRoutes, { educationInNumbersPageId });

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Manage Education in Numbers', link: '/education-in-numbers' },
        { name: 'Edit Education in Numbers page' },
      ]}
    >
      <LoadingSpinner loading={isLoading}>
        {educationInNumbersPage ? (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <PageTitle
                  metaTitle={`${pageRouteTitle} - ${educationInNumbersPage.title}`}
                  title={educationInNumbersPage.title}
                  caption="Edit Education in numbers Page"
                />
              </div>
            </div>

            {GetStatusTag(educationInNumbersPage)}

            <NavBar routes={navBarRoutes} label="EducationInNumbers" />

            <EducationInNumbersPageContextProvider
              educationInNumbersPage={educationInNumbersPage}
              onEducationInNumbersPageChange={nextEducationInNumbersPage => {
                setEducationInNumbersPage({
                  value: nextEducationInNumbersPage,
                });
              }}
            >
              <RouteSwitch routes={educationInNumbersPageRoutes} />
            </EducationInNumbersPageContextProvider>

            <PreviousNextLinks
              previousSection={previousSection}
              nextSection={nextSection}
            />
          </>
        ) : (
          <WarningMessage>
            Could not load Education in Numbers page
          </WarningMessage>
        )}
      </LoadingSpinner>
    </Page>
  );
};

function GetStatusTag({ published, version }: EinSummary) {
  if (published === undefined) {
    return version === 0 ? <Tag>Draft</Tag> : <Tag>Draft amendment</Tag>;
  }

  return <Tag>Published</Tag>;
}

export default EducationInNumbersPage;
