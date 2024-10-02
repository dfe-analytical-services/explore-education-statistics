import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import {
  releaseApiDataSetDetailsRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenLogRoute,
  ReleaseDataSetPreviewTokenRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import previewTokenQueries from '@admin/queries/previewTokenQueries';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import previewTokenService from '@admin/services/previewTokenService';
import { useLastLocation } from '@admin/contexts/LastLocationContext';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Button from '@common/components/Button';
import CopyTextButton from '@common/components/CopyTextButton';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import ApiDataSetQuickStart from '@common/modules/data-catalogue/components/ApiDataSetQuickStart';
import { useQuery } from '@tanstack/react-query';
import { generatePath, useHistory, useParams } from 'react-router-dom';
import React from 'react';

export default function ReleaseApiDataSetPreviewTokenPage() {
  const history = useHistory();
  const lastLocation = useLastLocation();

  const { publicApiUrl, publicApiDocsUrl } = useConfig();

  const { dataSetId, previewTokenId, releaseId, publicationId } =
    useParams<ReleaseDataSetPreviewTokenRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const { data: previewToken, isLoading: isLoadingPreviewTokenId } = useQuery({
    ...previewTokenQueries.get(previewTokenId),
  });

  const previewPagePath = generatePath<ReleaseDataSetRouteParams>(
    releaseApiDataSetPreviewRoute.path,
    {
      publicationId,
      releaseId,
      dataSetId,
    },
  );

  const detailsPagePath = generatePath<ReleaseDataSetRouteParams>(
    releaseApiDataSetDetailsRoute.path,
    {
      publicationId,
      releaseId,
      dataSetId,
    },
  );

  const tokenLogPagePath = generatePath<ReleaseDataSetRouteParams>(
    releaseApiDataSetPreviewTokenLogRoute.path,
    {
      publicationId,
      releaseId,
      dataSetId,
    },
  );

  const handleRevoke = async (id: string) => {
    await previewTokenService.revokePreviewToken(id);

    history.push(
      lastLocation?.pathname === tokenLogPagePath
        ? tokenLogPagePath
        : previewPagePath,
    );
  };

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={
          lastLocation?.pathname === tokenLogPagePath
            ? tokenLogPagePath
            : detailsPagePath
        }
      >
        {lastLocation?.pathname === tokenLogPagePath
          ? 'Back to API preview token log'
          : ' Back to API data set details'}
      </Link>
      <LoadingSpinner loading={isLoadingDataSet || isLoadingPreviewTokenId}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <span className="govuk-caption-l">API data set preview token</span>
            <h2>{dataSet?.title}</h2>

            {previewToken?.status === 'Active' ? (
              <>
                <p>
                  Token expiry time:{' '}
                  <strong>
                    <FormattedDate formatRelativeToNow>
                      {previewToken.expiry}
                    </FormattedDate>
                  </strong>
                </p>
                <h3>Using this token</h3>
                <h4>Step 1</h4>
                <p>TODO</p>
                <h4>Step 2</h4>
                <p>TODO else</p>
                <h4>Final checks</h4>
                <CopyTextButton
                  buttonText="Copy preview token"
                  className="govuk-!-margin-bottom-4"
                  confirmMessage="Token copied to the clipboard"
                  inlineButton={false}
                  label="Preview token"
                  text={previewToken.id}
                />
                <p>
                  Please revoke the token as soon as you have finished checking
                  the API data set.
                </p>
                <ModalConfirm
                  title="Revoke this token"
                  triggerButton={
                    <Button variant="warning">Revoke this token</Button>
                  }
                  onConfirm={() => handleRevoke(previewToken.id)}
                >
                  <p>Are you sure you want to revoke this token?</p>
                </ModalConfirm>
                <p>
                  <Link to={tokenLogPagePath}>View API data set token log</Link>
                </p>
                <h3>API data set endpoints quick start</h3>
                {dataSet?.draftVersion && (
                  <ApiDataSetQuickStart
                    publicApiBaseUrl={`${publicApiUrl}/api/v1.0`}
                    publicApiDocsUrl={publicApiDocsUrl}
                    dataSetId={dataSet.id}
                    dataSetName={dataSet.title}
                    dataSetVersion={dataSet.draftVersion?.version}
                    headingsTag="h4"
                    renderLink={linkProps => <Link {...linkProps} />}
                  />
                )}
              </>
            ) : (
              <p>This preview token has expired.</p>
            )}
          </div>
        </div>
      </LoadingSpinner>
    </>
  );
}
