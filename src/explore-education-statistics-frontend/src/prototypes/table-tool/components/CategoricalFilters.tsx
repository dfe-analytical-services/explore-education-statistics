import { FormFieldCheckboxGroup, FormFieldset } from '@common/components/form';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { FormValues } from '@frontend/prototypes/table-tool/components/FiltersForm';
import { MetaSpecification } from '@frontend/prototypes/table-tool/components/meta/initialSpec';
import SearchableGroupedFilterMenus from '@frontend/prototypes/table-tool/components/SearchableGroupedFilterMenus';
import { FormikState } from 'formik';
import React from 'react';
import styles from './CategoricalFilters.module.scss';

interface Props {
  form: FormikState<FormValues>;
  specification: MetaSpecification['categoricalFilters'];
}

const CategoricalFilters = ({ form, specification }: Props) => {
  const { getError } = createErrorHelper(form);

  return (
    <div className={styles.columns}>
      {Object.entries(specification).map(([filterKey, filterSpec]) => {
        const filterName = `categoricalFilters.${filterKey}`;

        return (
          <div className={styles.group} key={filterKey}>
            {Object.keys(filterSpec.options).length === 1 ? (
              <FormFieldCheckboxGroup<FormValues>
                id={`filter-${filterKey}`}
                name={filterName}
                legend={filterSpec.legend}
                hint={filterSpec.hint}
                options={filterSpec.options.default.options.map(option => ({
                  id: `${filterKey}-${option.value}`,
                  label: option.label,
                  value: option.value,
                }))}
                selectAll
                small
              />
            ) : (
              <FormFieldset
                id={`filter-${filterKey}`}
                legend={filterSpec.legend}
                hint={filterSpec.hint}
                error={getError(filterName)}
              >
                <SearchableGroupedFilterMenus<FormValues>
                  menuOptions={filterSpec.options}
                  name={filterName}
                  values={form.values.categoricalFilters[filterKey]}
                />
              </FormFieldset>
            )}
          </div>
        );
      })}
    </div>
  );
};

export default CategoricalFilters;
