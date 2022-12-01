import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import useToggle from '@common/hooks/useToggle';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import { ReleaseTypesModal } from '@frontend/modules/find-statistics/components/FilterModals';
import { FilterChangeHandler } from '@frontend/modules/find-statistics/components/Filters';
import React from 'react';

interface Props {
  releaseType?: ReleaseType | 'all';
  onChange: FilterChangeHandler;
}

const ReleaseTypeFilters = ({
  releaseType: initialReleaseType = 'all',
  onChange,
}: Props) => {
  const [showModal, toggleModal] = useToggle(false);
  const releaseTypeOptions = [
    { label: 'Show all', value: 'all' },
    ...Object.keys(releaseTypes).map(type => ({
      label: releaseTypes[type as ReleaseType],
      value: type,
    })),
  ];

  return (
    <form id="releaseTypeFilters">
      <FormRadioGroup
        hint={
          <ButtonText onClick={toggleModal.on}>
            What are release types?
          </ButtonText>
        }
        id="releaseType"
        legend="Filter by release type"
        legendHidden
        name="releaseType"
        options={releaseTypeOptions}
        order={[]}
        small
        value={initialReleaseType}
        onChange={e => {
          onChange({
            filterType: 'releaseType',
            nextValue: e.target.value,
          });
        }}
      />
      <Button className="dfe-js-hidden" type="submit">
        Submit
      </Button>

      <ReleaseTypesModal open={showModal} onClose={toggleModal.off} />
    </form>
  );
};

export default ReleaseTypeFilters;
