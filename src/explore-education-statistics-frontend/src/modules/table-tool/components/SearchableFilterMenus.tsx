import FormFieldCheckboxGroup from '@common/components/form/FormFieldCheckboxGroup';
import sortBy from 'lodash/sortBy';
import React, { PureComponent } from 'react';
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
    const { openFilters } = this.state;

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
          ) || openFilters[compositeKey],
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
                  ...openFilters,
                  [compositeKey]: isOpen,
                },
              });
            }}
          >
            <FormFieldCheckboxGroup
              legend="Choose option"
              legendHidden
              name={name as string}
              options={options}
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
