import WarningMessage from '@common/components/WarningMessage';
import PublicationForm, {
  PublicationFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationForm';
import Wizard, {
  InjectedWizardProps,
} from '@common/modules/table-tool/components/Wizard';
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
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import DownloadStep, {
  DownloadFormSubmitHandler,
} from '@frontend/modules/data-catalogue/components/DownloadStep';
import { ReleaseFormSubmitHandler } from '@frontend/modules/data-catalogue/components/ReleaseForm';
import ReleaseStep from '@frontend/modules/data-catalogue/components/ReleaseStep';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
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

const DataCataloguePage: NextPage<Props> = ({
  releases = [],
  selectedPublication,
  selectedRelease,
  subjects = [],
  themes,
}) => {
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
      throw new Error('Release has not been selected');
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

  const PublicationStep = (props: InjectedWizardProps) => {
    return (
      <PublicationForm
        {...props}
        initialValues={{
          publicationId: state.query.publication?.id ?? '',
        }}
        onSubmit={handlePublicationFormSubmit}
        themes={themes}
        renderSummaryAfter={
          state.query.publication?.isSuperseded &&
          state.query.publication.supersededBy ? (
            <WarningMessage testId="superseded-warning">
              This publication has been superseded by{' '}
              <Link
                testId="superseded-by-link"
                to={`/data-catalogue?publicationSlug=${state.query.publication.supersededBy.slug}`}
              >
                {state.query.publication.supersededBy.title}
              </Link>
            </WarningMessage>
          ) : null
        }
      />
    );
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
          {stepProps => <PublicationStep {...stepProps} />}
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
};

export const getServerSideProps: GetServerSideProps<Props> = async context => {
  const {
    publicationSlug = '',
    releaseSlug = '',
  } = context.query as Dictionary<string>;

  const themes = await publicationService.getPublicationTree({
    publicationFilter: 'DataCatalogue',
  });

  const selectedPublication = themes
    .flatMap(option => option.topics)
    .flatMap(option => option.publications)
    .find(option => option.slug === publicationSlug);

  let releases: ReleaseSummary[] = [];

  if (selectedPublication) {
    releases = await publicationService.listReleases(publicationSlug);
  }

  let selectedRelease: ReleaseSummary | undefined;

  if (releaseSlug) {
    selectedRelease = releases.find(rel => rel.slug === releaseSlug);
  }

  let subjects: Subject[] = [];

  if (selectedPublication && selectedRelease) {
    subjects = await tableBuilderService.listReleaseSubjects(
      selectedRelease.id,
    );
  }

  const props: Props = {
    releases,
    subjects,
    themes,
  };

  if (selectedPublication) {
    props.selectedPublication = selectedPublication;

    if (selectedRelease) {
      props.selectedRelease = selectedRelease;
    }
  }

  return {
    props,
  };
};

export default DataCataloguePage;
