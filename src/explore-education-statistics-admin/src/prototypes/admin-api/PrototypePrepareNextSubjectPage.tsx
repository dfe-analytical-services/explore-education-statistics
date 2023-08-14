import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Link from '@admin/components/Link';
import useStorageItem from '@common/hooks/useStorageItem';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import {
  subjectsForRelease1,
  PublicationSubject,
} from './PrototypePublicationSubjects';
import PrototypePrepareNextSubjectStep1 from './components/PrototypePrepareNextSubjectStep1';
import PrototypePrepareNextSubjectStep2 from './components/PrototypePrepareNextSubjectStep2';
import {
  unmappedLocations,
  newLocations,
  mappedLocations,
} from './data/locations';
import { unmappedFilters, newFilters, mappedFilters } from './data/filters';
import {
  unmappedIndicators,
  newIndicators,
  mappedIndicators,
} from './data/indicators';
import PrototypePrepareNextSubjectStep5 from './components/PrototypePrepareNextSubjectStep5';
import PrototypePrepareNextSubjectStep6 from './components/PrototypePrepareNextSubjectStep6';
import { PrototypeNextSubjectContextProvider } from './contexts/PrototypeNextSubjectContext';

export interface MapItem {
  id: string;
  label: string;
  code?: string;
  level?: string;
  region?: string;
  group?: string;
  filter?: string;
}

interface WizardState {
  initialStep: number;
}

interface MatchProps {
  id: string;
  psid: string;
}

const PrototypePrepareNextSubjectPage = ({
  match,
}: RouteComponentProps<MatchProps>) => {
  const [savedPublicationSubjects, setSavedPublicationSubjects] =
    useStorageItem<PublicationSubject[]>('publicationSubjects', undefined);

  const publicationSubjectId = match.params.psid;
  const publicationSubject = savedPublicationSubjects?.find(
    ps => ps.subjectId === publicationSubjectId,
  );
  const release = subjectsForRelease1.find(
    rel => rel.id === publicationSubject?.subjectId,
  );

  const initialState: WizardState = {
    initialStep: 1,
  };

  return (
    <PrototypeNextSubjectContextProvider
      locations={{
        newItems: newLocations,
        mappedItems: mappedLocations.map(item => {
          return [item, item];
        }),
        unmappedItems: unmappedLocations,
        noMappingItems: [],
      }}
      filters={{
        newItems: newFilters,
        mappedItems: mappedFilters.map(item => {
          return [item, item];
        }),
        unmappedItems: unmappedFilters,
        noMappingItems: [],
      }}
      indicators={{
        newItems: newIndicators,
        mappedItems: mappedIndicators.map(item => {
          return [item, item];
        }),
        unmappedItems: unmappedIndicators,
        noMappingItems: [],
      }}
      versionType="minor"
    >
      <Link
        className="govuk-!-margin-bottom-6 govuk-!-padding-left-3 govuk-link govuk-back-link"
        to="/prototypes/admin-api/data/2022-23#subjects"
      >
        Back to API data sets
      </Link>
      {publicationSubject && release ? (
        <section>
          <span className="govuk-caption-l">{publicationSubject.title}</span>
          <h2>Create new API data set version</h2>

          <SummaryList className="govuk-!-margin-bottom-8">
            <SummaryListItem term="Current data set (live)">
              {release.title}
            </SummaryListItem>
            <SummaryListItem term="Current data set version (live)">
              {release.version} <div className="govuk-tag">Live</div>
            </SummaryListItem>
            <SummaryListItem term="Current release (live)">
              {release.release}
            </SummaryListItem>
          </SummaryList>

          <Wizard
            initialStep={initialState.initialStep}
            id="dataCatalogueWizard"
          >
            <WizardStep size="l">
              {stepProps => (
                <PrototypePrepareNextSubjectStep1
                  onSubmit={subjectId => {
                    const updated = savedPublicationSubjects?.map(subject =>
                      subject.title === publicationSubject.title
                        ? { ...publicationSubject, nextSubjectId: subjectId }
                        : subject,
                    );
                    if (updated) {
                      setSavedPublicationSubjects(updated);
                    }
                  }}
                  {...stepProps}
                />
              )}
            </WizardStep>
            <WizardStep size="l">
              {stepProps => (
                <PrototypePrepareNextSubjectStep2
                  {...stepProps}
                  name="location"
                />
              )}
            </WizardStep>
            <WizardStep size="l">
              {stepProps => (
                <PrototypePrepareNextSubjectStep2
                  {...stepProps}
                  name="filter"
                />
              )}
            </WizardStep>
            <WizardStep size="l">
              {stepProps => (
                <PrototypePrepareNextSubjectStep2
                  {...stepProps}
                  name="indicator"
                />
              )}
            </WizardStep>
            <WizardStep size="l">
              {stepProps => <PrototypePrepareNextSubjectStep5 {...stepProps} />}
            </WizardStep>
            <WizardStep size="l">
              {stepProps => <PrototypePrepareNextSubjectStep6 {...stepProps} />}
            </WizardStep>
          </Wizard>
        </section>
      ) : (
        <p>No publication subject.</p>
      )}
    </PrototypeNextSubjectContextProvider>
  );
};
export default PrototypePrepareNextSubjectPage;
