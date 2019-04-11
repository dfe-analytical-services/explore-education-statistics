import { FormGroup } from '@common/components/form';
import FormFieldCheckboxGroup from '@common/components/form/FormFieldCheckboxGroup';
import MenuDetails from '@frontend/modules/table-tool/components/MenuDetails';
import { GroupedFilterOptions } from '@frontend/prototypes/table-tool/components/meta/initialSpec';
import SearchTextInput from '@frontend/prototypes/table-tool/components/SearchTextInput';
import classNames from 'classnames';
import camelCase from 'lodash/camelCase';
import sortBy from 'lodash/sortBy';
import React, { PureComponent } from 'react';
import styles from './SearchableGroupedFilterMenus.module.scss';

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
    const { openFilters, searchTerm } = this.state;

    const containsSearchTerm = (value: string) =>
      value.search(new RegExp(searchTerm, 'i')) > -1;

    // Remove dots from field name as this does not work well in Robot tests
    const compositeKeyName = (name as string).replace('.', '-');

    const groups = sortBy(Object.entries(menuOptions), ([groupKey]) => groupKey)
      .filter(
        ([_, group]) =>
          searchTerm === '' ||
          group.options.some(
            item =>
              containsSearchTerm(item.label) || values.indexOf(item.value) > -1,
          ),
      )
      .map(([groupKey, group]) => {
        const compositeKey = `${compositeKeyName}-${camelCase(groupKey)}`;

        const isMenuOpen = Boolean(
          group.options.some(
            item => searchTerm !== '' && containsSearchTerm(item.label),
          ) || openFilters[compositeKey],
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

        const optionsSelected = group.options.reduce(
          (acc, option) => (values.indexOf(option.value) > -1 ? acc + 1 : acc),
          0,
        );

        return (
          <MenuDetails
            summary={
              <>
                {groupKey}
                {optionsSelected > 0 && (
                  <span
                    className={classNames('govuk-tag', styles.selectionCount)}
                  >
                    {optionsSelected} selected
                  </span>
                )}
              </>
            }
            key={compositeKey}
            open={isMenuOpen}
            onToggle={isOpen => {
              this.setState(prevState => ({
                openFilters: {
                  ...prevState.openFilters,
                  [compositeKey]: isOpen,
                },
              }));
            }}
          >
            <FormFieldCheckboxGroup
              name={name as string}
              options={options}
              id={compositeKey}
              selectAll
              small
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
