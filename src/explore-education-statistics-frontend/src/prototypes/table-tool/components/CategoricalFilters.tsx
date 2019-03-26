import { FormikState } from 'formik';
import React from 'react';
import { FormFieldCheckboxGroup, FormFieldset } from 'src/components/form';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import { FormValues } from 'src/prototypes/table-tool/components/FiltersForm';
import { MetaSpecification } from 'src/prototypes/table-tool/components/meta/initialSpec';
import SearchableGroupedFilterMenus from 'src/prototypes/table-tool/components/SearchableGroupedFilterMenus';
import styles from './CategoricalFilters.module.scss';

interface Props {
  form: FormikState<FormValues>;
  specification: MetaSpecification['categoricalFilters'];
}

const CategoricalFilters = ({ form, specification }: Props) => {
  const { getError } = createErrorHelper<any>(form);

  return (
    <div className={styles.columns}>
      {Object.entries(specification).map(([filterKey, filterSpec]) => {
        return (
          <div className={styles.group} key={filterKey}>
            {Array.isArray(filterSpec.options) ? (
              <FormFieldCheckboxGroup<FormValues>
                id={`filter-${filterKey}`}
                name={filterKey}
                legend={filterSpec.legend}
                hint={filterSpec.hint}
                options={filterSpec.options.map(option => ({
                  id: `${filterKey}-${option.name}`,
                  label: option.label,
                  value: option.name,
                }))}
                selectAll
              />
            ) : (
              <FormFieldset
                id={`filter-${filterKey}`}
                legend={filterSpec.legend}
                hint={filterSpec.hint}
                error={getError(filterKey)}
              >
                <SearchableGroupedFilterMenus<FormValues>
                  menuOptions={filterSpec.options}
                  name={filterKey}
                  values={form.values.characteristics}
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
