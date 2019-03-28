import camelCase from 'lodash/camelCase';
import sortBy from 'lodash/sortBy';
import React, { PureComponent } from 'react';
import { FormGroup } from 'src/components/form';
import FormFieldCheckboxGroup from 'src/components/form/FormFieldCheckboxGroup';
import MenuDetails from 'src/modules/table-tool/components/MenuDetails';
import { GroupedFilterOptions } from 'src/prototypes/table-tool/components/meta/initialSpec';
import SearchTextInput from 'src/prototypes/table-tool/components/SearchTextInput';

interface Props<FormValues> {
  menuOptions: GroupedFilterOptions;
  name: keyof FormValues | string;
  values: string[];
}

interface State {
  openFilters: {
    [name: string]: boolean;
  };
  searchTerm: string;
}

class SearchableGroupedFilterMenus<
  FormValues extends {
    [key: string]: unknown;
  }
> extends PureComponent<Props<FormValues>, State> {
  public static defaultProps = {
    values: [],
  };

  public state: State = {
    openFilters: {},
    searchTerm: '',
  };

  public render() {
    const { menuOptions, name, values } = this.props;
    const { searchTerm } = this.state;

    const containsSearchTerm = (value: string) =>
      value.search(new RegExp(searchTerm, 'i')) > -1;

    const groups = sortBy(Object.entries(menuOptions), ([groupKey]) => groupKey)
      .filter(
        ([groupKey, group]) =>
          searchTerm === '' ||
          group.options.some(
            item =>
              containsSearchTerm(item.label) || values.indexOf(item.value) > -1,
          ),
      )
      .map(([groupKey, group]) => {
        const compositeKey = `${name}-${camelCase(groupKey)}`;

        const isMenuOpen = Boolean(
          group.options.some(
            item =>
              (searchTerm !== '' && containsSearchTerm(item.label)) ||
              values.indexOf(item.value) > -1,
          ) || this.state.openFilters[compositeKey],
        );

        const options = sortBy(
          group.options
            .filter(
              item =>
                searchTerm === '' ||
                containsSearchTerm(item.label) ||
                values.indexOf(item.value) > -1,
            )
            .map(item => ({
              id: `${compositeKey}-${item.value}`,
              label: item.label,
              value: item.value,
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
              id={compositeKey}
              selectAll
              showError={false}
            />
          </MenuDetails>
        );
      });

    return (
      <>
        <FormGroup>
          <SearchTextInput
            id={`search-${name}`}
            label="Search for an option"
            name={`search-${name}`}
            onChange={event =>
              this.setState({ searchTerm: event.target.value })
            }
          />
        </FormGroup>
        <FormGroup>
          {groups.length > 0 ? groups : `No options matching '${searchTerm}'.`}
        </FormGroup>
      </>
    );
  }
}

export default SearchableGroupedFilterMenus;
