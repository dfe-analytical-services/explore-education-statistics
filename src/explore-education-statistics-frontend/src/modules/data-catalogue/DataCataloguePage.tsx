import themeService, { DownloadTheme } from '@common/services/themeService';
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
import ErrorPage from '@frontend/modules/ErrorPage';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
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
    published: '2021-01-01T11:21:17.7585345',
    slug: 'rel-3-slug',
    title: 'Another Release',
  } as Release,
  {
    id: 'rel-2',
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
  themes: DownloadTheme[];
}

interface DataCatalogueState {
  initialStep: number;
  releases: Release[];
  query: {
    release?: Release;
    subjects: SubjectWithDownloadFiles[];
  };
}

const DataCataloguePage: NextPage<Props> = ({ themes }: Props) => {
  const router = useRouter();

  const [state, updateState] = useImmer<DataCatalogueState>({
    initialStep: 1,
    releases: [],
    query: {
      release: undefined,
      subjects: [],
    },
  });

  const handlePublicationStepBack = () => {
    router.push('/data-catalogue', undefined, { shallow: true });
  };

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async () => {
    updateState(draft => {
      draft.releases = fakeReleases;
    });
  };

  const handleReleaseFormSubmit: ReleaseFormSubmitHandler = async ({
    releaseId: selectedReleaseId,
  }) => {
    updateState(draft => {
      draft.query.release = draft.releases.find(
        rel => rel.id === selectedReleaseId,
      );
      draft.query.subjects = fakeSubjectsWithDownloadFiles;
    });
  };

  const handleDownloadFormSubmit: DownloadFormSubmitHandler = ({ files }) => {
    // EES-2007  DO STUFF!
  };

  // EES-2007 temp until page is complete
  if (process.env.NODE_ENV === 'production') {
    return <ErrorPage statusCode={404} />;
  }

  const PublicationStep = (props: InjectedWizardProps) => {
    const options: DownloadTheme[] = themes;
    return (
      <PublicationForm
        {...props}
        onSubmit={handlePublicationFormSubmit}
        options={options}
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
              subjects={state.query.subjects}
              onSubmit={handleDownloadFormSubmit}
            />
          )}
        </WizardStep>
      </Wizard>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async () => {
  const themes =
    process.env.NODE_ENV !== 'production'
      ? await themeService.getDownloadThemes()
      : []; // EES-2007 temp until page is complete
  return {
    props: {
      themes,
    },
  };
};

export default DataCataloguePage;
