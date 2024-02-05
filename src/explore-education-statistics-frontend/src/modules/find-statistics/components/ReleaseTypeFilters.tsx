import Button from '@common/components/Button';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
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
        hint={<ReleaseTypesModal />}
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
    </form>
  );
};

export default ReleaseTypeFilters;
