import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useAuthContext } from '@admin/contexts/AuthContext';
import ReleaseMetaGuidanceDataFile from '@admin/pages/release/components/ReleaseMetaGuidanceDataFile';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseMetaGuidanceService, {
  ReleaseMetaGuidance,
} from '@admin/services/releaseMetaGuidanceService';
import releaseService, { Release } from '@admin/services/releaseService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SanitizeHtml from '@common/components/SanitizeHtml';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { RouteComponentProps, StaticContext } from 'react-router';

interface LocationState {
  backLink: string;
}

interface Model {
  metaGuidance: ReleaseMetaGuidance;
  release: Release;
}

const ReleaseMetaGuidancePage = ({
  match,
  location,
}: RouteComponentProps<ReleaseRouteParams, StaticContext, LocationState>) => {
  const { releaseId } = match.params;

  const { user } = useAuthContext();

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [metaGuidance, release] = await Promise.all([
      releaseMetaGuidanceService.getMetaGuidance(releaseId),
      releaseService.getRelease(releaseId),
    ]);

    return {
      metaGuidance,
      release,
    };
  }, [releaseId]);

  return (
    <Page
      wide
      breadcrumbs={
        user && user.permissions.canAccessAnalystPages
          ? [{ name: 'Metadata guidance document' }]
          : []
      }
      homePath={user?.permissions.canAccessAnalystPages ? '/' : ''}
      backLink={location.state?.backLink}
    >
      <LoadingSpinner loading={isLoading}>
        {model && (
          <>
            <PageTitle
              title={model.release.publicationTitle}
              caption={model.release.title}
            />

            <h2>Metadata guidance document</h2>

            {model.release.published && (
              <p>
                <strong>
                  Published{' '}
                  <FormattedDate>{model.release.published}</FormattedDate>
                </strong>
              </p>
            )}

            <SanitizeHtml dirtyHtml={model.metaGuidance.content} />

            {model.metaGuidance.subjects.length > 0 && (
              <>
                <h3 className="govuk-!-margin-top-6">Data files</h3>

                <Accordion id="dataFiles">
                  {model.metaGuidance.subjects.map(subject => (
                    <AccordionSection heading={subject.name} key={subject.id}>
                      <ReleaseMetaGuidanceDataFile
                        key={subject.id}
                        subject={subject}
                      />
                    </AccordionSection>
                  ))}
                </Accordion>
              </>
            )}
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default ReleaseMetaGuidancePage;
