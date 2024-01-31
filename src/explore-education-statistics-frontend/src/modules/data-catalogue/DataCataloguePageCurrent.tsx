import SubmitError from '@common/components/form/util/SubmitError';
import WarningMessage from '@common/components/WarningMessage';
import PublicationForm, {
  PublicationFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationForm';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import downloadService from '@common/services/downloadService';
import publicationService, {
  PublicationTreeSummary,
  ReleaseSummary,
  Theme,
} from '@common/services/publicationService';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import Page from '@frontend/components/Page';
import DownloadStep, {
  DownloadFormSubmitHandler,
} from '@frontend/modules/data-catalogue/components/DownloadStep';
import { ReleaseFormSubmitHandler } from '@frontend/modules/data-catalogue/components/ReleaseForm';
import ReleaseStep from '@frontend/modules/data-catalogue/components/ReleaseStep';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { useRouter } from 'next/router';
import React, { useEffect, useMemo } from 'react';
import { useImmer } from 'use-immer';

interface Props {
  releases?: ReleaseSummary[];
  selectedPublication?: PublicationTreeSummary;
  selectedRelease?: ReleaseSummary;
  subjects?: Subject[];
  themes: Theme[];
}

interface DataCatalogueState {
  initialStep: number;
  releases: ReleaseSummary[];
  subjects: Subject[];
  query: {
    publication?: PublicationTreeSummary;
    release?: ReleaseSummary;
  };
}

export default function DataCataloguePageCurrent({
  releases = [],
  selectedPublication,
  selectedRelease,
  subjects = [],
  themes,
}: Props) {
  const router = useRouter();

  const initialState = useMemo<DataCatalogueState>(() => {
    const getInitialStep = () => {
      if (selectedPublication && selectedRelease) {
        return 3;
      }
      if (selectedPublication) {
        return 2;
      }
      return 1;
    };

    return {
      initialStep: getInitialStep(),
      releases,
      subjects,
      query: {
        publication: selectedPublication,
        release: selectedRelease,
      },
    };
  }, [releases, selectedPublication, selectedRelease, subjects]);

  const [state, updateState] = useImmer<DataCatalogueState>(initialState);

  useEffect(() => {
    updateState(() => initialState);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [router.query.publicationSlug, updateState]);

  const handlePublicationStepBack = () => {
    router.push('/data-catalogue');
    updateState(draft => {
      // ensure no stale state is left in query
      draft.query.release = undefined;
      draft.query.publication = undefined;
    });
  };

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publication,
  }) => {
    const nextReleases = await publicationService.listReleases(
      publication.slug,
    );

    updateState(draft => {
      draft.releases = nextReleases;
      draft.query.publication = publication;
    });
  };

  const handleReleaseFormSubmit: ReleaseFormSubmitHandler = async ({
    release,
  }) => {
    const nextSubjects = await tableBuilderService.listReleaseSubjects(
      release.id,
    );

    updateState(draft => {
      draft.query.release = release;
      draft.subjects = nextSubjects;
    });
  };

  const handleDownloadFormSubmit: DownloadFormSubmitHandler = async ({
    files,
  }) => {
    const { release } = state.query;

    if (!release) {
      throw new SubmitError('Release has not been selected');
    }

    await downloadService.downloadFiles(release.id, files);

    const availableFiles = state.subjects.map(subject => subject.file);
    const selectedFilenames = files
      .map(fileId => availableFiles.find(file => file.id === fileId)?.fileName)
      .join(', ');

    logEvent({
      category: 'Downloads',
      action: 'Data Catalogue page selected files download',
      label: `Publication: ${state.query.publication?.title}, Release: ${release.title} File: ${selectedFilenames}`,
    });
  };

  return (
    <Page
      caption="Data catalogue"
      title="Browse our open data"
      breadcrumbLabel="Data catalogue"
    >
      <p className="govuk-body-l">
        View all of the open data available and choose files to download.
      </p>

      <Wizard initialStep={state.initialStep} id="dataCatalogueWizard">
        <WizardStep onBack={handlePublicationStepBack}>
          {stepProps => (
            <PublicationForm
              {...stepProps}
              initialValues={{
                publicationId: state.query.publication?.id ?? '',
              }}
              showSupersededPublications
              onSubmit={handlePublicationFormSubmit}
              themes={themes}
              renderSummaryAfter={
                state.query.publication?.isSuperseded &&
                state.query.publication.supersededBy ? (
                  <WarningMessage testId="superseded-warning">
                    This publication has been superseded by{' '}
                    <a
                      data-testid="superseded-by-link"
                      href={`/data-catalogue?publicationSlug=${state.query.publication.supersededBy.slug}`}
                    >
                      {state.query.publication.supersededBy.title}
                    </a>
                  </WarningMessage>
                ) : null
              }
            />
          )}
        </WizardStep>
        <WizardStep>
          {stepProps => (
            <ReleaseStep
              {...stepProps}
              releases={state.releases}
              selectedRelease={state.query.release}
              onSubmit={handleReleaseFormSubmit}
              hideLatestDataTag={state.query.publication?.isSuperseded}
            />
          )}
        </WizardStep>
        <WizardStep>
          {stepProps => (
            <DownloadStep
              {...stepProps}
              release={state.query.release}
              subjects={state.subjects}
              onSubmit={handleDownloadFormSubmit}
              hideLatestDataTag={state.query.publication?.isSuperseded}
            />
          )}
        </WizardStep>
      </Wizard>
    </Page>
  );
}
