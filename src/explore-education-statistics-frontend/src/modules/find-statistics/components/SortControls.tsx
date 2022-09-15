import Button from '@common/components/Button';
import { Form, FormRadioGroup, FormSelect } from '@common/components/form';
import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationSortOption } from '@common/services/publicationService';
import styles from '@frontend/modules/find-statistics/components/SortControls.module.scss';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';

const options = [
  { label: 'Newest', value: 'newest' },
  { label: 'Oldest', value: 'oldest' },
  { label: 'A to Z', value: 'alphabetical' },
];

interface Props {
  sortBy: PublicationSortOption;
  onChange: (nextSortBy: PublicationSortOption) => void;
}

const SortControls = ({ sortBy, onChange }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.container}>
      <Formik initialValues={{ sortBy }} onSubmit={noop}>
        <Form id="sortControlsForm">
          {isMobileMedia ? (
            <FormSelect
              id="sortControls"
              inline
              name="sortBy"
              label="Sort results"
              options={options}
              order={[]}
              value={sortBy}
              onChange={event =>
                onChange(event.target.value as PublicationSortOption)
              }
            />
          ) : (
            <FormRadioGroup
              id="sortControls"
              inline
              legend="Sort results"
              legendSize="s"
              name="sortBy"
              options={options}
              order={[]}
              small
              value={sortBy}
              onChange={event =>
                onChange(event.target.value as PublicationSortOption)
              }
            />
          )}
          <Button className="dfe-js-hidden" type="submit">
            Submit
          </Button>
        </Form>
      </Formik>
    </div>
  );
};

export default SortControls;
