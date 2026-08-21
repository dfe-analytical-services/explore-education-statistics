import NavBar from '@admin/components/NavBar';
import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import PreviousNextLinks from '@admin/components/PreviousNextLinks';
import RouteSwitch from '@admin/components/RouteSwitch';
import { MethodologyContextProvider } from '@admin/pages/methodology/contexts/MethodologyContext';
import getMethodologyApprovalStatusLabel from '@admin/pages/methodology/utils/getMethodologyApprovalStatusLabel';
import methodologyPageRoutes from '@admin/routes/methodologyPageRoutes';
import {
  methodologyNavRoutes,
  MethodologyRouteParams,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import useNavRoutes from '@admin/hooks/useNavRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const MethodologyPage = ({
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const {
    value: methodology,
    setState: setMethodology,
    isLoading,
  } = useAsyncHandledRetry(() => {
    return methodologyService.getMethodology(methodologyId);
  }, [methodologyId]);

  const { navBarRoutes, currentRouteTitle, previousSection, nextSection } =
    useNavRoutes(methodologyNavRoutes, { methodologyId });

  return (
    <Page wide breadcrumbs={[{ name: 'Edit methodology' }]}>
      <LoadingSpinner loading={isLoading}>
        {methodology ? (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <PageTitle
                  metaTitle={
                    currentRouteTitle
                      ? `${currentRouteTitle} - ${methodology.title}`
                      : methodology.title
                  }
                  title={methodology.title}
                  caption={
                    methodology.amendment
                      ? 'Amend methodology'
                      : 'Edit methodology'
                  }
                />
              </div>
            </div>

            <Tag>{getMethodologyApprovalStatusLabel(methodology.status)}</Tag>

            {methodology.amendment && (
              <Tag className="govuk-!-margin-left-2">Amendment</Tag>
            )}

            <NavBar routes={navBarRoutes} label="Methodology" />

            <MethodologyContextProvider
              methodology={methodology}
              onMethodologyChange={nextMethodology => {
                setMethodology({ value: nextMethodology });
              }}
            >
              <RouteSwitch routes={methodologyPageRoutes} />
            </MethodologyContextProvider>

            <PreviousNextLinks
              previousSection={previousSection}
              nextSection={nextSection}
            />
          </>
        ) : (
          <WarningMessage>Could not load methodology</WarningMessage>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default MethodologyPage;
