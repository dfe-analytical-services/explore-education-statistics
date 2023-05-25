import PrototypeFrontEndPage from '@admin/prototypes/components/PrototypeFrontEndPage';
import PrototypeTableToolFinalStep from '@admin/prototypes/components/PrototypeTableToolFinalStep';
import PrototypeTableToolWizard from '@admin/prototypes/components/PrototypeTableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import {
  FastTrackTable,
  FeaturedTable,
  SelectedPublication,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { Theme } from '@common/services/publicationService';
import Link from '@admin/components/Link';
import styles from '@admin/prototypes/PrototypePublicPage.module.scss';
import { themes } from '@admin/prototypes/data/newThemesData';
import React, { useState } from 'react';

export interface Props {
  fastTrack?: FastTrackTable;
  featuredTables?: FeaturedTable[];
  selectedPublication?: SelectedPublication;
  subjects?: Subject[];
  subjectMeta?: SubjectMeta;
  themeMeta: Theme[];
}

const PrototypeTableTool = ({ fastTrack, themeMeta = themes }: Props) => {
  const initialState = undefined;

  const [loadingFastTrack, setLoadingFastTrack] = useState(false);

  return (
    <div className={(styles.prototypePublicPage, styles.prototypeTableTool)}>
      <PrototypeFrontEndPage
        caption="Table Tool"
        title="Create your own tables"
        wide
      >
        <p>
          Choose the data and area of interest you want to explore and then use
          filters to create your table.
          <br />
          Once you've created your table, you can download the data it contains
          for your own offline analysis.
        </p>

        <PrototypeTableToolWizard
          key={fastTrack?.id}
          scrollOnMount
          themeMeta={themeMeta}
          initialState={initialState}
          loadingFastTrack={loadingFastTrack}
          renderFeaturedTable={highlight => (
            <Link
              to={`/data-tables/fast-track/${highlight.id}`}
              onClick={() => {
                setLoadingFastTrack(true);
              }}
            >
              {highlight.name}
            </Link>
          )}
          finalStep={({
            query,
            response,
            selectedPublication: selectedPublicationDetails,
          }) => (
            <WizardStep size="l">
              {wizardStepProps => (
                <>
                  <WizardStepHeading {...wizardStepProps} isActive>
                    Explore data
                  </WizardStepHeading>

                  {response && query && selectedPublicationDetails && (
                    <PrototypeTableToolFinalStep
                      query={query}
                      table={response.table}
                      tableHeaders={response.tableHeaders}
                      selectedPublication={selectedPublicationDetails}
                    />
                  )}
                </>
              )}
            </WizardStep>
          )}
        />
      </PrototypeFrontEndPage>
    </div>
  );
};

export default PrototypeTableTool;
