import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useGetChartFile from '@admin/hooks/useGetChartFile';
import KeyStatistics from '@admin/pages/release/content/components/KeyStatistics';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import focusAddedSectionBlockButton from '@admin/utils/focus/focusAddedSectionBlockButton';
import Button from '@common/components/Button';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import VisuallyHidden from '@common/components/VisuallyHidden';
import DataBlockTabs from '@common/modules/find-statistics/components/DataBlockTabs';
import React, { useCallback, useRef } from 'react';
import AddSecondaryStats from './AddSecondaryStats';

interface Props {
  release: EditableRelease;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}

const ReleaseHeadlines = ({ release, transformFeaturedTableLinks }: Props) => {
  const { editingMode } = useEditingContext();
  const actions = useReleaseContentActions();

  const addHeadlinesBlockButton = useRef<HTMLButtonElement>(null);

  const getChartFile = useGetChartFile(release.id);

  const addBlock = useCallback(async () => {
    const newBlock = await actions.addContentSectionBlock({
      releaseVersionId: release.id,
      sectionId: release.headlinesSection.id,
      sectionKey: 'headlinesSection',
      block: {
        type: 'HtmlBlock',
        order: 0,
        body: '',
      },
    });

    focusAddedSectionBlockButton(newBlock.id);
  }, [actions, release.id, release.headlinesSection.id]);

  const onAfterDeleteHeadlinesBlock = () => {
    setTimeout(() => {
      addHeadlinesBlockButton.current?.focus();
    }, 100);
  };

  const headlinesTab = (
    <TabsSection title="Summary">
      <section data-scroll id="releaseHeadlines-keyStatistics">
        <KeyStatistics release={release} isEditing={editingMode === 'edit'} />
      </section>
      <section id="releaseHeadlines-headlines">
        <EditableSectionBlocks
          blocks={release.headlinesSection.content}
          renderBlock={block => (
            <ReleaseBlock
              block={block}
              releaseVersionId={release.id}
              transformFeaturedTableLinks={transformFeaturedTableLinks}
            />
          )}
          renderEditableBlock={block => (
            <ReleaseEditableBlock
              allowComments
              block={block}
              editButtonLabel={
                <>
                  Edit<VisuallyHidden> headlines</VisuallyHidden> block
                </>
              }
              removeButtonLabel={
                <>
                  Remove<VisuallyHidden> headlines</VisuallyHidden> block
                </>
              }
              publicationId={release.publication.id}
              releaseVersionId={release.id}
              sectionId={release.headlinesSection.id}
              sectionKey="headlinesSection"
              onAfterDeleteBlock={onAfterDeleteHeadlinesBlock}
            />
          )}
        />

        {editingMode === 'edit' &&
          release.headlinesSection.content?.length === 0 && (
            <div className="govuk-!-margin-bottom-8 govuk-!-text-align-centre">
              <Button
                variant="secondary"
                onClick={addBlock}
                ref={addHeadlinesBlockButton}
              >
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
        Headline facts and figures - {release.yearTitle}
      </h2>

      {release.keyStatisticsSecondarySection.content?.length ? (
        <>
          <AddSecondaryStats release={release} updating />
          <DataBlockTabs
            releaseVersionId={release.id}
            id="releaseHeadlines-dataBlock"
            dataBlock={release.keyStatisticsSecondarySection.content[0]}
            getInfographic={getChartFile}
            firstTabs={headlinesTab}
          />
        </>
      ) : (
        <>
          <AddSecondaryStats release={release} />
          <Tabs id="releaseHeadlines-dataBlock">{headlinesTab}</Tabs>
        </>
      )}
    </section>
  );
};

export default ReleaseHeadlines;
