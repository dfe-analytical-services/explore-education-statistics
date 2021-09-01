import themeService, {
  DownloadTheme,
  PublicationDownloadSummary,
  PublicationSummary,
  Theme,
} from '@common/services/themeService';
import Page from '@frontend/components/Page';
import PublicationForm, {
  PublicationFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationForm';
import { Release } from '@common/services/publicationService';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import Wizard, {
  InjectedWizardProps,
} from '@common/modules/table-tool/components/Wizard';
import ReleaseStep from '@frontend/modules/data-catalogue/components/ReleaseStep';
import { ReleaseFormSubmitHandler } from '@frontend/modules/data-catalogue/components/ReleaseForm';
import DownloadStep, {
  DownloadFormSubmitHandler,
  SubjectWithDownloadFiles,
} from '@frontend/modules/data-catalogue/components/DownloadStep';
import { Dictionary } from '@common/types';
import ErrorPage from '@frontend/modules/ErrorPage';
import { GetServerSideProps, NextPage } from 'next';
import React, { useMemo } from 'react';
import { useRouter } from 'next/router';
import { useImmer } from 'use-immer';

const fakeReleases: Release[] = [
  {
    id: 'rel-1',
    latestRelease: true,
    published: '2021-06-30T11:21:17.7585345',
    slug: 'rel-1-slug',
    title: 'Release 1',
  } as Release,
  {
    id: 'rel-3',
    latestRelease: false,
    published: '2021-01-01T11:21:17.7585345',
    slug: 'rel-3-slug',
    title: 'Another Release',
  } as Release,
  {
    id: 'rel-2',
    latestRelease: false,
    published: '2021-05-30T11:21:17.7585345',
    slug: 'rel-2-slug',
    title: 'Release 2',
  } as Release,
];
const fakeSubjectsWithDownloadFiles: SubjectWithDownloadFiles[] = [
  {
    id: 'subject-1',
    name: 'Subject 1',
    content: 'Some content here',
    geographicLevels: ['SoYo'],
    timePeriods: {
      from: '2016',
      to: '2019',
    },
    downloadFile: {
      id: 'file-1',
      extension: 'csv',
      fileName: 'file-1.csv',
      name: 'File 1',
      size: '100mb',
      type: 'Data',
    },
  },
  {
    id: 'subject-2',
    name: 'Another Subject',
    content: 'Some content here',
    geographicLevels: ['SoYo'],
    timePeriods: {
      from: '2016',
      to: '2019',
    },
    downloadFile: {
      id: 'file-2',
      extension: 'csv',
      fileName: 'file-2.csv',
      name: 'File 2',
      size: '100mb',
      type: 'Data',
    },
  },
  {
    id: 'subject-3',
    name: 'Subject 3',
    content: 'Some content here',
    geographicLevels: ['SoYo'],
    timePeriods: {
      from: '2016',
      to: '2019',
    },
    downloadFile: {
      id: 'file-3',
      extension: 'csv',
      fileName: 'file-3.csv',
      name: 'File 3',
      size: '100mb',
      type: 'Data',
    },
  },
];

interface Props {
  releases?: Release[];
  selectedPublication?: PublicationSummary;
  selectedRelease?: Release;
  subjects?: SubjectWithDownloadFiles[];
  themes: Theme[];
}

interface DataCatalogueState {
  initialStep: number;
  releases: Release[];
  subjects: SubjectWithDownloadFiles[];
  query: {
    publicationId?: string;
    release?: Release;
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
        publicationId: selectedPublication?.id || '',
        release: (selectedPublication && selectedRelease) || undefined,
      },
    };
  }, [releases, selectedPublication, selectedRelease, subjects]);

  const [state, updateState] = useImmer<DataCatalogueState>(initialState);

  const handlePublicationStepBack = () => {
    router.push('/data-catalogue', undefined, { shallow: true });
  };

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publicationId,
  }) => {
    updateState(draft => {
      draft.releases = fakeReleases;
      draft.query.publicationId = publicationId;
    });
  };

  const handleReleaseFormSubmit: ReleaseFormSubmitHandler = async ({
    releaseId: selectedReleaseId,
  }) => {
    updateState(draft => {
      draft.query.release = draft.releases.find(
        rel => rel.id === selectedReleaseId,
      );
      draft.subjects = fakeSubjectsWithDownloadFiles;
    });
  };

  const handleDownloadFormSubmit: DownloadFormSubmitHandler = ({ files }) => {
    // EES-2007  DO STUFF!
  };

  // EES-2007 temp until page is complete
  if (process.env.APP_ENV === 'Production') {
    return <ErrorPage statusCode={404} />;
  }

  const PublicationStep = (props: InjectedWizardProps) => {
    return (
      <PublicationForm
        {...props}
        initialValues={{
          publicationId: state.query.publicationId ?? '',
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
  // EES-2007 temp until page is complete
  if (process.env.APP_ENV === 'Production') {
    // eslint-disable-next-line no-param-reassign
    context.res.statusCode = 404;
    return {
      props: {
        themes: [],
      },
    };
  }

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

  // EES-2007 Get real releases here
  const releases = fakeReleases;

  let selectedRelease: Release | undefined;
  if (releaseSlug) {
    selectedRelease = releases.find(rel => rel.slug === releaseSlug);
  }

  let subjects;
  if (selectedPublication && selectedRelease) {
    // EES-2007 Get real subjects here
    subjects = fakeSubjectsWithDownloadFiles;
  }

  const props: Props = {
    releases: releases ?? [],
    subjects: subjects ?? [],
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
