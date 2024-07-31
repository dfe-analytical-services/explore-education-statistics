import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import KeyStatistics from '@admin/pages/release/content/components/KeyStatistics';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableReleaseVersion } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import React, { useCallback } from 'react';
import AddSecondaryStats from './AddSecondaryStats';

interface Props {
  releaseVersion: EditableReleaseVersion;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}

const ReleaseHeadlines = ({
  releaseVersion,
  transformFeaturedTableLinks,
}: Props) => {
  const { editingMode } = useEditingContext();
  const actions = useReleaseContentActions();

  const getChartFile = useGetChartFile(releaseVersion.id);

  const addBlock = useCallback(async () => {
    await actions.addContentSectionBlock({
      releaseVersionId: releaseVersion.id,
      sectionId: releaseVersion.headlinesSection.id,
      sectionKey: 'headlinesSection',
      block: {
        type: 'HtmlBlock',
        order: 0,
        body: '',
      },
    });
  }, [actions, releaseVersion.id, releaseVersion.headlinesSection.id]);

  const headlinesTab = (
    <TabsSection title="Summary">
      <section data-scroll id="releaseHeadlines-keyStatistics">
        <KeyStatistics
          release={releaseVersion}
          isEditing={editingMode === 'edit'}
        />
      </section>
      <section id="releaseHeadlines-headlines">
        <EditableSectionBlocks
          blocks={releaseVersion.headlinesSection.content}
          sectionId={releaseVersion.headlinesSection.id}
          renderBlock={block => (
            <ReleaseBlock
              block={block}
              releaseVersionId={releaseVersion.id}
              transformFeaturedTableLinks={transformFeaturedTableLinks}
            />
          )}
          renderEditableBlock={block => (
            <ReleaseEditableBlock
              allowComments
              block={block}
              publicationId={releaseVersion.publication.id}
              releaseVersionId={releaseVersion.id}
              sectionId={releaseVersion.headlinesSection.id}
              sectionKey="headlinesSection"
            />
          )}
        />

        {editingMode === 'edit' &&
          releaseVersion.headlinesSection.content?.length === 0 && (
            <div className="govuk-!-margin-bottom-8 govuk-!-text-align-centre">
              <Button variant="secondary" onClick={addBlock}>
                Add a headlines text block
              </Button>
            </div>
          )}
      </section>
    </TabsSection>
  );

  return (
    <section id="releaseHeadlines">
      <h2
        className="dfe-print-break-before"
        data-scroll
        id="release-headlines-header"
      >
        Headline facts and figures - {releaseVersion.yearTitle}
      </h2>

      {releaseVersion.keyStatisticsSecondarySection.content?.length ? (
        <>
          <AddSecondaryStats releaseVersion={releaseVersion} updating />
          {/* TODO rename to releaseVersionId */}
          <DataBlockTabs
            releaseId={releaseVersion.id}
            id="releaseHeadlines-dataBlock"
            dataBlock={releaseVersion.keyStatisticsSecondarySection.content[0]}
            getInfographic={getChartFile}
            firstTabs={headlinesTab}
          />
        </>
      ) : (
        <>
          <AddSecondaryStats releaseVersion={releaseVersion} />
          <Tabs id="releaseHeadlines-dataBlock">{headlinesTab}</Tabs>
        </>
      )}
    </section>
  );
};

export default ReleaseHeadlines;
