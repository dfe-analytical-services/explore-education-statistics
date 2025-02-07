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
import CodeBlock from '@common/components/CodeBlock';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Button from '@common/components/Button';
import CopyTextButton from '@common/components/CopyTextButton';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ApiDataSetQuickStart from '@common/modules/data-catalogue/components/ApiDataSetQuickStart';
import { useQuery } from '@tanstack/react-query';
import { generatePath, useHistory, useParams } from 'react-router-dom';
import React from 'react';

export default function ReleaseApiDataSetPreviewTokenPage() {
  const history = useHistory();
  const lastLocation = useLastLocation();

  const { publicApiUrl, publicApiDocsUrl } = useConfig();

  const { dataSetId, previewTokenId, releaseVersionId, publicationId } =
    useParams<ReleaseDataSetPreviewTokenRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const { data: previewToken, isLoading: isLoadingPreviewTokenId } = useQuery({
    ...previewTokenQueries.get(previewTokenId),
  });

  const detailsPagePath = generatePath<ReleaseDataSetRouteParams>(
    releaseApiDataSetDetailsRoute.path,
    {
      publicationId,
      releaseVersionId,
      dataSetId,
    },
  );

  const tokenLogPagePath = generatePath<ReleaseDataSetRouteParams>(
    releaseApiDataSetPreviewTokenLogRoute.path,
    {
      publicationId,
      releaseVersionId,
      dataSetId,
    },
  );

  const handleRevoke = async (id: string) => {
    await previewTokenService.revokePreviewToken(id);
    history.push(tokenLogPagePath);
  };

  const tokenExampleUrl = `${publicApiUrl}/v1/data-sets/${dataSet?.draftVersion?.id}`;

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
          <div className="govuk-grid-column-three-quarters-from-desktop">
            <span className="govuk-caption-l">API data set preview token</span>
            <h2>{dataSet?.title}</h2>

            {previewToken?.status === 'Active' ? (
              <>
                <p>
                  Reference: <strong>{previewToken.label}</strong>
                </p>

                <CopyTextButton
                  buttonText="Copy preview token"
                  confirmText="Preview token copied"
                  id="copy-preview-token"
                  label="Preview token"
                  text={previewToken.id}
                />

                <p>
                  The token expires:{' '}
                  <strong>
                    <FormattedDate formatRelativeToNow>
                      {previewToken.expiry}
                    </FormattedDate>{' '}
                    (local time)
                  </strong>
                </p>

                <h3>Using the preview token</h3>

                <p>
                  To use the preview token, add it to your request using a{' '}
                  <code>Preview-Token</code> header.
                </p>
                <p>
                  The examples below illustrate how you can add the preview
                  token header to your code.
                </p>

                <Tabs id="preview-token-examples">
                  <TabsSection title="cURL" headingTitle="cURL" headingTag="h4">
                    <CodeBlock language="bash">
                      {`curl -X GET -H "Preview-Token: ${previewToken.id}" \\
  ${tokenExampleUrl}`}
                    </CodeBlock>
                  </TabsSection>
                  <TabsSection
                    title="Python"
                    headingTitle="Python"
                    headingTag="h4"
                  >
                    <CodeBlock language="python">
                      {`import requests

url = "${tokenExampleUrl}"

response = requests.get(url, headers={"Preview-Token": "${previewToken.id}"})`}
                    </CodeBlock>
                  </TabsSection>
                  <TabsSection title="R" headingTitle="R" headingTag="h4">
                    <CodeBlock language="r">{`library(httr)

url <- "${tokenExampleUrl}"

response <- GET(url, add_headers("Preview-Token" = "${previewToken.id}"))
`}</CodeBlock>
                  </TabsSection>
                </Tabs>

                <h3>Revoking the preview token</h3>

                <p>
                  Once you have checked the API data set, it is recommended that
                  you revoke the preview token.
                </p>

                <ModalConfirm
                  title="Revoke preview token"
                  triggerButton={
                    <Button variant="warning">Revoke preview token</Button>
                  }
                  onConfirm={() => handleRevoke(previewToken.id)}
                >
                  <p>Are you sure you want to revoke the preview token?</p>
                </ModalConfirm>

                <p>
                  <Link to={tokenLogPagePath}>View preview token log</Link>
                </p>

                <h3>API data set endpoints quick start</h3>

                {dataSet?.draftVersion && (
                  <ApiDataSetQuickStart
                    publicApiBaseUrl={publicApiUrl}
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
