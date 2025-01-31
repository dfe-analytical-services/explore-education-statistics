import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
import PageTitle from '@admin/components/PageTitle';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import { MethodologyRouteParams } from '@admin/routes/methodologyRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import {
  MethodologyContentProvider,
  useMethodologyContentState,
} from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContext';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import methodologyQueries from '@admin/queries/methodologyQueries';
import methodologyContentQueries from '@admin/queries/methodologyContentQueries';
import permissionQueries from '@admin/queries/permissionQueries';
import MethodologyContent from './components/MethodologyContent';

export const MethodologyContentPageInternal = () => {
  const {
    methodology,
    methodologyVersion,
    canUpdateMethodology,
    isPreRelease,
  } = useMethodologyContentState();

  const canUpdateContent = !isPreRelease && canUpdateMethodology;

  return (
    <EditingContextProvider editingMode={canUpdateContent ? 'edit' : 'preview'}>
      {canUpdateContent && <EditablePageModeToggle />}

      <div className="govuk-width-container">
        {isPreRelease ? (
          <PageTitle caption="Methodology" title={methodology.title} />
        ) : (
          <h2 aria-hidden className="govuk-heading-lg" data-testid="page-title">
            {methodology.title}
          </h2>
        )}

        <MethodologyContent
          methodology={methodology}
          methodologyVersion={methodologyVersion}
        />
      </div>
    </EditingContextProvider>
  );
};

const MethodologyContentPage = ({
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const { data: methodologyVersion, isLoading: isMethodologyVersionLoading } =
    useQuery(methodologyQueries.get(methodologyId));

  const { data: methodologyContent, isLoading: isMethodologyContentLoading } =
    useQuery(methodologyContentQueries.get(methodologyId));

  const {
    data: canUpdateMethodology,
    isLoading: isCanUpdateMethodologyLoading,
  } = useQuery(permissionQueries.canUpdateMethodology(methodologyId));

  const isLoading =
    isMethodologyVersionLoading ||
    isMethodologyContentLoading ||
    isCanUpdateMethodologyLoading;

  return (
    <LoadingSpinner loading={isLoading}>
      {methodologyContent &&
      methodologyVersion &&
      canUpdateMethodology !== undefined ? (
        <MethodologyContentProvider
          value={{
            methodology: methodologyContent,
            methodologyVersion,
            canUpdateMethodology,
          }}
        >
          <MethodologyContentPageInternal />
        </MethodologyContentProvider>
      ) : (
        <WarningMessage>Could not load methodology</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default MethodologyContentPage;
