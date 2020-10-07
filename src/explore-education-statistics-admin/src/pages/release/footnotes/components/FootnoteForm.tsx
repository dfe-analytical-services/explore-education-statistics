import FilterGroupDetails from '@admin/pages/release/footnotes/components/FilterGroupDetails';
import IndicatorDetails from '@admin/pages/release/footnotes/components/IndicatorDetails';
import {
  BaseFootnote,
  Footnote,
  FootnoteMeta,
} from '@admin/services/footnoteService';
import footnoteToFlatFootnote from '@admin/services/utils/footnote/footnoteToFlatFootnote';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldCheckbox } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import get from 'lodash/get';
import orderBy from 'lodash/orderBy';
import React, { ReactNode } from 'react';

interface Props {
  cancelButton?: ReactNode;
  footnote?: Footnote;
  footnoteMeta: FootnoteMeta;
  id?: string;
  onSubmit: (values: BaseFootnote) => void;
}

const FootnoteForm = ({
  cancelButton,
  footnote,
  footnoteMeta,
  id = 'footnoteForm',
  onSubmit,
}: Props) => {
  const getInitialValues = (): BaseFootnote => {
    /**
     * This function can be removed once the id's of
     * filters, filterItems etc. aren't numbers
     * We can just use the initialValue below.
     */
    const initialValue: BaseFootnote = {
      content: '',
      subjects: {},
    };

    Object.values(footnoteMeta.subjects).map(subject => {
      const { subjectId } = subject;

      initialValue.subjects[subjectId] = {
        indicatorGroups: {},
        filters: {},
        selected: false,
      };
      Object.keys(subject.indicators).map(indicatorId => {
        initialValue.subjects[subjectId].indicatorGroups[`${indicatorId}`] = {
          selected: false,
          indicators: [],
        };
        return null;
      });
      return Object.entries(subject.filters).map(([filterId, filter]) => {
        initialValue.subjects[subjectId].filters[`${filterId}`] = {
          selected: false,
          filterGroups: {},
        };

        return Object.keys(filter.options).map(filterGroupId => {
          initialValue.subjects[subjectId].filters[`${filterId}`].filterGroups[
            `${filterGroupId}`
          ] = {
            selected: false,
            filterItems: [],
          };
          return null;
        });
      });
    });

    return initialValue;
  };

  return (
    <Formik<BaseFootnote>
      initialValues={footnote || getInitialValues()}
      validationSchema={Yup.object<BaseFootnote>({
        content: Yup.string().required('Footnote content must be added.'),
        subjects: Yup.object(),
      })}
      onSubmit={async values => {
        const {
          subjects,
          indicatorGroups,
          indicators,
          filters,
          filterGroups,
          filterItems,
        } = footnoteToFlatFootnote(values);

        const hasNoneSelected =
          [
            ...subjects,
            ...indicators,
            ...indicatorGroups,
            ...filters,
            ...filterGroups,
            ...filterItems,
          ].length === 0;

        if (hasNoneSelected) {
          throw new Error(
            'At least one Subject, Indicator or Filter must be selected',
          );
        }

        await onSubmit(values);
      }}
    >
      {form => (
        <Form id={id} showSubmitError>
          <p>
            Select filters and indicators from either one or multiple subject
            areas and then add your footnote (shown at the bottom of the page).
          </p>

          {orderBy(
            Object.values(footnoteMeta.subjects),
            subject => subject.subjectName,
          ).map(subject => {
            const { subjectId, subjectName } = subject;

            const subjectSelected =
              get(form.values, `subjects.${subjectId}.selected`) || false;

            return (
              <fieldset
                key={subjectId}
                className="govuk-fieldset"
                data-testid={`footnote-subject ${subjectName}`}
              >
                <legend className="govuk-heading-m">
                  Subject: {subjectName}
                </legend>

                <FormFieldCheckbox
                  id={`subject-${subjectId}`}
                  label="Select all indicators and filters for this subject"
                  name={`subjects.${subjectId}.selected`}
                  small
                />

                <div className="govuk-grid-row govuk-!-margin-top-3 ">
                  <div className="govuk-grid-column-one-half">
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                      Indicators
                    </h3>

                    <IndicatorDetails
                      summary="Indicators"
                      parentSelected={subjectSelected}
                      valuePath={`subjects.${subjectId}`}
                      indicatorGroups={subject.indicators}
                      form={form}
                    />
                  </div>

                  <div className="govuk-grid-column-one-half">
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                      Filters
                    </h3>

                    {Object.entries(subject.filters).map(
                      ([filterId, filter]) => (
                        <FilterGroupDetails
                          key={filterId}
                          summary={filter.legend}
                          parentSelected={subjectSelected}
                          valuePath={`subjects.${subjectId}`}
                          groupId={filterId}
                          filter={filter}
                          selectAll
                          value={get(
                            form.values,
                            `subjects.${subjectId}.filters.${filterId}.selected`,
                          )}
                          form={form}
                        />
                      ),
                    )}
                  </div>
                </div>
                <hr className="govuk-!-margin-bottom-2" />
              </fieldset>
            );
          })}

          <FormFieldTextArea<BaseFootnote>
            id={`${id}-content`}
            name="content"
            label="Footnote"
          />

          <ButtonGroup>
            <Button type="submit">Save footnote</Button>
            {cancelButton}
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default FootnoteForm;
