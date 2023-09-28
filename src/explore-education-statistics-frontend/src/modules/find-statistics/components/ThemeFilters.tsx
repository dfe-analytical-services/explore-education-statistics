import Button from '@common/components/Button';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import { ThemeSummary } from '@common/services/themeService';
import { ThemesModal } from '@frontend/modules/find-statistics/components/FilterModals';
import { FilterChangeHandler } from '@frontend/modules/find-statistics/components/FiltersDesktop';
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
        hint={<ThemesModal themes={themes} />}
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
    </form>
  );
};

export default ThemeFilters;
