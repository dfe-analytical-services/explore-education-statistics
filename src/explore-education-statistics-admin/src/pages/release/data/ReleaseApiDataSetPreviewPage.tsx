import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import {
  releaseApiDataSetDetailsRoute,
  releaseApiDataSetPreviewTokenRoute,
  ReleaseDataSetPreviewTokenRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import ApiDataSetPreviewTokenCreateForm from '@admin/pages/release/data/components/ApiDataSetPreviewTokenCreateForm';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import previewTokenService from '@admin/services/previewTokenService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useToggle from '@common/hooks/useToggle';
import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import { useQuery } from '@tanstack/react-query';
import { generatePath, useHistory, useParams } from 'react-router-dom';
import React from 'react';
import { PreviewTokenCreateValues } from '@admin/pages/release/data/types/PreviewTokenCreateValues';
import PreviewTokenDateHelper from '@admin/pages/release/data/utils/previewTokenDateHelper';

export default function ReleaseApiDataSetPreviewPage() {
  const history = useHistory();
  const { publicApiDocsUrl } = useConfig();

  const [modalOpen, toggleModalOpen] = useToggle(false);

  const { dataSetId, releaseVersionId, publicationId } =
    useParams<ReleaseDataSetPreviewTokenRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const handleCreate = async ({
    label,
    datePresetSpan,
    activates,
    expires,
  }: PreviewTokenCreateValues) => {
    if (!dataSet?.draftVersion) {
      return;
    }

    const tokenDatesHelper = new PreviewTokenDateHelper();
    const { startDate, endDate } =
      datePresetSpan && datePresetSpan > 0
        ? tokenDatesHelper.setDateRangeBasedOnPresets(datePresetSpan)
        : tokenDatesHelper.setDateRangeBasedOnCustomDates(activates, expires);

    const token = await previewTokenService.createPreviewToken({
      label,
      dataSetVersionId: dataSet?.draftVersion?.id,
      activates: startDate,
      expires: endDate,
    });

    history.push(
      generatePath<ReleaseDataSetPreviewTokenRouteParams>(
        releaseApiDataSetPreviewTokenRoute.path,
        {
          publicationId,
          releaseVersionId,
          dataSetId,
          previewTokenId: token.id,
        },
      ),
    );
  };

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseDataSetRouteParams>(
          releaseApiDataSetDetailsRoute.path,
          {
            publicationId,
            releaseVersionId,
            dataSetId,
          },
        )}
      >
        Back to API data set details
      </Link>
      <LoadingSpinner loading={isLoadingDataSet}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <span className="govuk-caption-l">
              Generate API data set preview token
            </span>
            <h2>{dataSet?.title}</h2>

            <p>This API data set version is ready to be published.</p>
            <p>
              You can preview the data by generating a token. The preview token
              will allow you to query the data via the public API using your
              tool of choice.
            </p>
            <p>
              The preview token should be treated as unpublished data, do not
              share it with anyone who doesn't already have access to the
              unpublished data in the API data set.
            </p>
            <p>
              The preview token will be valid for <strong>24 hours</strong>{' '}
              after creation.
            </p>
            <p>
              <Link to={publicApiDocsUrl ?? ''}>View API documentation</Link>
            </p>
            <Modal
              open={modalOpen}
              title="Generate preview token"
              triggerButton={
                <Button
                  className="govuk-!-margin-top-5"
                  onClick={toggleModalOpen.on}
                >
                  Generate preview token
                </Button>
              }
            >
              <p>
                Preview tokens are valid for <strong>24 hours</strong> by
                default to meet pre-release access needs.
              </p>
              <p>
                If you require a longer duration preview token for testing of
                secondary products such as data dashboards, contact{' '}
                <a href="mailto:explore.statistics@education.gov.uk">
                  {' '}
                  explore.statistics@education.gov.uk
                </a>
                .
              </p>
              <ApiDataSetPreviewTokenCreateForm
                onCancel={toggleModalOpen.off}
                onSubmit={handleCreate}
              />
            </Modal>
          </div>
        </div>
      </LoadingSpinner>
    </>
  );
}
