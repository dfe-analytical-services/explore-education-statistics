import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import useToggle from '@common/hooks/useToggle';
import { ThemeSummary } from '@common/services/themeService';
import { ThemesModal } from '@frontend/modules/find-statistics/components/FilterModals';
import { FilterChangeHandler } from '@frontend/modules/find-statistics/components/Filters';
import React from 'react';

interface Props {
  themeId?: string;
  themes: ThemeSummary[];
  onChange: FilterChangeHandler;
}

const ThemeFilters = ({
  themeId: initialThemeId = 'all',
  themes,
  onChange,
}: Props) => {
  const [showModal, toggleModal] = useToggle(false);
  const themeFilters = [
    { label: 'All themes', value: 'all' },
    ...themes.map(theme => ({
      label: theme.title,
      value: theme.id,
    })),
  ];

  return (
    <form id="themeFilters">
      <FormRadioGroup
        hint={
          <ButtonText onClick={toggleModal.on}>What are themes?</ButtonText>
        }
        id="theme"
        legend="Filter by theme"
        name="theme"
        options={themeFilters}
        small
        value={initialThemeId}
        order={[]}
        onChange={e => {
          onChange({ filterType: 'themeId', nextValue: e.target.value });
        }}
      />
      <Button className="dfe-js-hidden" type="submit">
        Submit
      </Button>
      <ThemesModal open={showModal} themes={themes} onClose={toggleModal.off} />
    </form>
  );
};

export default ThemeFilters;
