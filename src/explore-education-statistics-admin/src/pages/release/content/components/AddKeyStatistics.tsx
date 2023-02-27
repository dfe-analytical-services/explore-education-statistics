import KeyStatDataBlockSelectForm from '@admin/pages/release/content/components/KeyStatDataBlockSelectForm';
import styles from '@admin/pages/release/content/components/KeyStatistics.module.scss';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import React, { useCallback, useState } from 'react';
import { KeyStatisticType } from '@common/services/publicationService';
import { KeyStatisticTextCreateRequest } from '@admin/services/keyStatisticService';
import EditableKeyStatTextForm from '@admin/pages/release/content/components/EditableKeyStatTextForm';
import ButtonGroup from '@common/components/ButtonGroup';
import { KeyStatisticsProps } from '@admin/pages/release/content/components/KeyStatistics';

const AddKeyStatistics = ({ release }: KeyStatisticsProps) => {
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
      await addKeyStatisticDataBlock({ releaseId: release.id, dataBlockId });
      await updateUnattachedDataBlocks({ releaseId: release.id });
      setFormType(undefined);
    },
    [release.id, addKeyStatisticDataBlock, updateUnattachedDataBlocks],
  );

  const addKeyStatText = useCallback(
    async (newKeyStatText: KeyStatisticTextCreateRequest) => {
      await addKeyStatisticText({
        releaseId: release.id,
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
            releaseId={release.id}
            onSelect={addKeyStatDataBlock}
            onCancel={() => setFormType(undefined)}
          />
        </div>
      );
    case 'KeyStatisticText':
      return (
        <div className={styles.textFormContainer}>
          <EditableKeyStatTextForm
            onSubmit={values => addKeyStatText(values)}
            onCancel={() => setFormType(undefined)}
            testId="keyStatText-createForm"
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
};

export default AddKeyStatistics;
