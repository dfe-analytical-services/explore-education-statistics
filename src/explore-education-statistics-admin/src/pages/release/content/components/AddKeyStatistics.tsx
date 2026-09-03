import KeyStatDataBlockSelectForm from '@admin/pages/release/content/components/KeyStatDataBlockSelectForm';
import styles from '@admin/pages/release/content/components/KeyStatistics.module.scss';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import React, { useCallback, useState } from 'react';
import { KeyStatisticType } from '@common/services/publicationService';
import { KeyStatisticTextCreateRequest } from '@admin/services/keyStatisticService';
import EditableKeyStatTextForm from '@admin/pages/release/content/components/EditableKeyStatTextForm';
import ButtonGroup from '@common/components/ButtonGroup';

interface Props {
  release: EditableRelease;
}

export default function AddKeyStatistics({ release }: Props) {
  const [formType, setFormType] = useState<KeyStatisticType | undefined>(
    undefined,
  );
  const {
    updateUnattachedDataBlocks,
    addKeyStatisticDataBlock,
    addKeyStatisticText,
  } = useReleaseContentActions();

  const addKeyStatDataBlock = useCallback(
    async (dataBlockId: string) => {
      await addKeyStatisticDataBlock({
        releaseVersionId: release.id,
        dataBlockId,
      });
      await updateUnattachedDataBlocks({ releaseVersionId: release.id });
      setFormType(undefined);
    },
    [release.id, addKeyStatisticDataBlock, updateUnattachedDataBlocks],
  );

  const addKeyStatText = useCallback(
    async (newKeyStatText: KeyStatisticTextCreateRequest) => {
      await addKeyStatisticText({
        releaseVersionId: release.id,
        keyStatisticText: newKeyStatText,
      });
      setFormType(undefined);
    },
    [release.id, addKeyStatisticText],
  );

  switch (formType) {
    case 'KeyStatisticDataBlock':
      return (
        <div className={styles.dataBlockFormContainer}>
          <WarningMessage>
            In order to add a key statistic from a data block, you first need to
            create a data block with just one value.
            <br />
            Any data blocks with more than one value cannot be selected as a key
            statistic.
          </WarningMessage>
          <KeyStatDataBlockSelectForm
            releaseVersionId={release.id}
            onSelect={addKeyStatDataBlock}
            onCancel={() => setFormType(undefined)}
          />
        </div>
      );
    case 'KeyStatisticText':
      return (
        <div className={styles.textFormContainer}>
          <EditableKeyStatTextForm
            keyStats={release.keyStatistics}
            testId="keyStatText-createForm"
            onSubmit={values => addKeyStatText(values)}
            onCancel={() => setFormType(undefined)}
          />
        </div>
      );
    default:
      return (
        <ButtonGroup>
          <Button
            onClick={() => {
              setFormType('KeyStatisticDataBlock');
            }}
            className="govuk-!-margin-bottom-2"
          >
            Add key statistic from data block
          </Button>
          <Button
            onClick={() => {
              setFormType('KeyStatisticText');
            }}
            className="govuk-!-margin-bottom-2"
          >
            Add free text key statistic
          </Button>
        </ButtonGroup>
      );
  }
}
