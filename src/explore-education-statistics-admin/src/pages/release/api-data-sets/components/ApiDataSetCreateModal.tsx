import Link from '@admin/components/Link';
import ApiDataSetCreateForm, {
  ApiDataSetCreateFormValues,
} from '@admin/pages/release/api-data-sets/components/ApiDataSetCreateForm';
import apiDataSetCandidateQueries from '@admin/queries/apiDataSetCandidateQueries';
import {
  releaseApiDataSetDetailsRoute,
  releaseDataRoute,
  ReleaseDataSetRouteParams,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetService from '@admin/services/apiDataSetService';
import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { useQuery } from '@tanstack/react-query';
import React from 'react';
import { generatePath, useHistory } from 'react-router-dom';

interface Props {
  publicationId: string;
  releaseId: string;
}

export default function ApiDataSetCreateModal({
  publicationId,
  releaseId,
}: Props) {
  const history = useHistory();
  const [isOpen, toggleOpen] = useToggle(false);

  const {
    data: dataSetCandidates = [],
    isLoading,
    refetch,
  } = useQuery(apiDataSetCandidateQueries.list(releaseId));

  const handleTriggerClick = async () => {
    toggleOpen.on();
    await refetch();
  };

  const handleSubmit = async ({
    releaseFileId,
  }: ApiDataSetCreateFormValues) => {
    const dataSet = await apiDataSetService.createDataSet({
      releaseFileId,
    });

    history.push(
      generatePath<ReleaseDataSetRouteParams>(
        releaseApiDataSetDetailsRoute.path,
        {
          publicationId,
          releaseId,
          dataSetId: dataSet.id,
        },
      ),
    );
  };

  if (isLoading) {
    return null;
  }

  return (
    <Modal
      open={isOpen}
      title="Create a new API data set"
      triggerButton={
        <Button onClick={handleTriggerClick}>Create API data set</Button>
      }
    >
      <p>
        Select a data set to become an API data set. This will be made available
        for third-party applications to consume via the public API.
      </p>

      {dataSetCandidates.length > 0 ? (
        <ApiDataSetCreateForm
          dataSetCandidates={dataSetCandidates}
          onCancel={toggleOpen.off}
          onSubmit={handleSubmit}
        />
      ) : (
        <>
          <WarningMessage>
            No API data sets can be created as there are no candidate data files
            available. New candidate data files can be uploaded in the{' '}
            <Link
              to={generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
                publicationId,
                releaseId,
              })}
            >
              Data and files
            </Link>{' '}
            section.
          </WarningMessage>

          <Button onClick={toggleOpen.off}>Close</Button>
        </>
      )}
    </Modal>
  );
}
