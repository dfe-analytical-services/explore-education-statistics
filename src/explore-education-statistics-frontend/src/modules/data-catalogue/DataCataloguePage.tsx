import PublicationForm, {
  PublicationFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationForm';
import Wizard, {
  InjectedWizardProps,
} from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import downloadService from '@common/services/downloadService';
import publicationService, {
  ReleaseSummary,
} from '@common/services/publicationService';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import themeService, {
  PublicationSummary,
  Theme,
} from '@common/services/themeService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import DownloadStep, {
  DownloadFormSubmitHandler,
} from '@frontend/modules/data-catalogue/components/DownloadStep';
import { ReleaseFormSubmitHandler } from '@frontend/modules/data-catalogue/components/ReleaseForm';
import ReleaseStep from '@frontend/modules/data-catalogue/components/ReleaseStep';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import { useRouter } from 'next/router';
import React, { useMemo } from 'react';
import { useImmer } from 'use-immer';

interface Props {
  releases?: ReleaseSummary[];
  selectedPublication?: PublicationSummary;
  selectedRelease?: ReleaseSummary;
  subjects?: Subject[];
  themes: Theme[];
}

interface DataCatalogueState {
  initialStep: number;
  releases: ReleaseSummary[];
  subjects: Subject[];
  query: {
    publication?: PublicationSummary;
    release?: ReleaseSummary;
  };
}

const DataCataloguePage: NextPage<Props> = ({
  releases = [],
  selectedPublication,
  selectedRelease,
  subjects = [],
  themes,
}: Props) => {
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

  const handlePublicationStepBack = () => {
    router.push('/data-catalogue', undefined, { shallow: true });
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
        options={themes}
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

  const themes = await themeService.listThemes({
    publicationFilter: 'AnyData',
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
