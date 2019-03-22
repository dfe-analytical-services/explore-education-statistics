import sortBy from 'lodash/sortBy';
import React, { PureComponent } from 'react';
import FormFieldCheckboxGroup from 'src/components/form/FormFieldCheckboxGroup';
import MenuDetails from './MenuDetails';

interface Props<FormValues> {
  menuOptions: {
    [menuGroup: string]: {
      label: string;
      name: string;
    }[];
  };
  name: keyof FormValues;
  searchTerm: string;
  values: string[];
}

interface State {
  openFilters: {
    [name: string]: boolean;
  };
}

class SearchableFilterMenus<
  FormValues extends {
    [key: string]: unknown;
  }
> extends PureComponent<Props<FormValues>, State> {
  public state: State = {
    openFilters: {},
  };

  public render() {
    const { menuOptions, name, searchTerm, values } = this.props;

    const containsSearchTerm = (value: string) =>
      value.search(new RegExp(searchTerm, 'i')) > -1;

    const groups = Object.entries(menuOptions)
      .filter(
        ([groupKey]) =>
          searchTerm === '' ||
          menuOptions[groupKey].some(
            item =>
              containsSearchTerm(item.label) || values.indexOf(item.name) > -1,
          ),
      )
      .map(([groupKey, items]) => {
        const compositeKey = `${name}-${groupKey}`;

        const isMenuOpen = Boolean(
          menuOptions[groupKey].some(
            item =>
              (searchTerm !== '' && containsSearchTerm(item.label)) ||
              values.indexOf(item.name) > -1,
          ) || this.state.openFilters[compositeKey],
        );

        const options = sortBy(
          items
            .filter(
              item =>
                searchTerm === '' ||
                containsSearchTerm(item.label) ||
                values.indexOf(item.name) > -1,
            )
            .map(item => ({
              id: item.name,
              label: item.label,
              value: item.name,
            })),
          ['label'],
        );

        return (
          <MenuDetails
            summary={groupKey}
            key={compositeKey}
            open={isMenuOpen}
            onToggle={isOpen => {
              this.setState({
                openFilters: {
                  ...this.state.openFilters,
                  [compositeKey]: isOpen,
                },
              });
            }}
          >
            <FormFieldCheckboxGroup
              name={name as string}
              options={options}
              value={values}
              id={compositeKey}
              selectAll
              showError={false}
            />
          </MenuDetails>
        );
      });

    return groups.length > 0 ? groups : `No options matching '${searchTerm}'.`;
  }
}

export default SearchableFilterMenus;
