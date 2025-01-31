import Link from '@admin/components/Link';
import ApiDataSetCreateForm, {
  ApiDataSetCreateFormValues,
} from '@admin/pages/release/data/components/ApiDataSetCreateForm';
import apiDataSetCandidateQueries from '@admin/queries/apiDataSetCandidateQueries';
import {
  releaseDataRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { useQuery } from '@tanstack/react-query';
import React, { ReactNode } from 'react';
import { generatePath } from 'react-router-dom';
import releaseDataPageTabIds from '@admin/pages/release/data/utils/releaseDataPageTabIds';

interface Props {
  buttonText?: ReactNode | string;
  publicationId: string;
  releaseId: string;
  submitText?: string;
  title?: string;
  onSubmit: (values: ApiDataSetCreateFormValues) => void;
}

export default function ApiDataSetCreateModal({
  buttonText = 'Create API data set',
  publicationId,
  releaseId,
  submitText,
  title = 'Create a new API data set',
  onSubmit,
}: Props) {
  const [isOpen, toggleOpen] = useToggle(false);

  const { data: dataSetCandidates = [], isLoading } = useQuery({
    ...apiDataSetCandidateQueries.list(releaseId),
    enabled: isOpen,
  });

  return (
    <Modal
      open={isOpen}
      title={title}
      triggerButton={<Button onClick={toggleOpen.on}>{buttonText}</Button>}
    >
      <LoadingSpinner loading={isLoading}>
        <p>
          Select a data set to become an API data set. This will be made
          available for third-party applications to consume via the public API.
        </p>

        {dataSetCandidates.length > 0 ? (
          <ApiDataSetCreateForm
            dataSetCandidates={dataSetCandidates}
            submitText={submitText}
            onCancel={toggleOpen.off}
            onSubmit={onSubmit}
          />
        ) : (
          <>
            <WarningMessage>
              No API data sets can be created as there are no candidate data
              files available. New candidate data files can be uploaded in the{' '}
              <Link
                to={`${generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
                  publicationId,
                  releaseId,
                })}#${releaseDataPageTabIds.dataUploads}`}
                onClick={toggleOpen.off}
              >
                Data and files
              </Link>{' '}
              section.
            </WarningMessage>

            <Button onClick={toggleOpen.off}>Close</Button>
          </>
        )}
      </LoadingSpinner>
    </Modal>
  );
}
