import FilterGroupDetails from '@admin/pages/release/footnotes/components/FilterGroupDetails';
import IndicatorDetails from '@admin/pages/release/footnotes/components/IndicatorDetails';
import styles from '@admin/pages/release/footnotes/components/FootnoteForm.module.scss';
import {
  BaseFootnote,
  Footnote,
  FootnoteMeta,
  SubjectSelectionType,
} from '@admin/services/footnoteService';
import footnoteToFlatFootnote from '@admin/services/utils/footnote/footnoteToFlatFootnote';
import {
  pluginsConfigLinksOnly,
  toolbarConfigLinkOnly,
} from '@admin/config/ckEditorConfig';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import sanitizeHtml from '@common/utils/sanitizeHtml';
import SubmitError from '@common/components/form/util/SubmitError';
import RHFFormFieldEditor from '@admin/components/form/RHFFormFieldEditor';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import Yup from '@common/validation/yup';
import deepmerge from 'deepmerge';
import mapValues from 'lodash/mapValues';
import orderBy from 'lodash/orderBy';
import React, { ReactNode, useMemo } from 'react';
import { ObjectSchema } from 'yup';
import VisuallyHidden from '@common/components/VisuallyHidden';

interface Props {
  cancelButton?: ReactNode;
  footnote?: Footnote;
  footnoteMeta: FootnoteMeta;
  id?: string;
  onSubmit: (values: BaseFootnote) => void | Promise<void>;
}

export default function FootnoteForm({
  cancelButton,
  footnote,
  footnoteMeta,
  id = 'footnoteForm',
  onSubmit,
}: Props) {
  const initialValues = useMemo<BaseFootnote>(() => {
    const subjects = mapValues(footnoteMeta.subjects, subject => {
      const { indicators, filters, subjectId } = subject;

      let selectionType: SubjectSelectionType = 'NA';

      if (footnote && Object.keys(footnote.subjects).includes(subjectId)) {
        const foundSubject = footnote.subjects[subjectId];
        if (foundSubject.selected) {
          selectionType = 'All';
        } else if (
          Object.keys(foundSubject.filters).length ||
          Object.keys(foundSubject.indicatorGroups).length
        ) {
          selectionType = 'Specific';
        }
      }

      return {
        selected: false,
        selectionType,
        indicatorGroups: mapValues(indicators, () => ({
          selected: false,
          indicators: [],
        })),
        filters: mapValues(filters, filter => ({
          selected: false,
          filterGroups: mapValues(filter.options, () => ({
            selected: false,
            filterItems: [],
          })),
        })),
      };
    });

    return {
      content: footnote?.content ?? '',
      subjects: deepmerge(subjects, footnote?.subjects ?? {}),
    };
  }, [footnote, footnoteMeta.subjects]);

  const handleSubmit = async (values: BaseFootnote) => {
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
      throw new SubmitError(
        'At least one Subject, Indicator or Filter must be selected',
      );
    }

    const sanitizedValues = {
      ...values,
      content: sanitizeHtml(values.content, { allowedTags: ['a'] }),
    };

    await onSubmit(sanitizedValues);
  };

  const validationSchema = useMemo<ObjectSchema<BaseFootnote>>(() => {
    return Yup.object({
      content: Yup.string().required('Footnote content must be added'),
      subjects: Yup.object(),
    });
  }, []);

  return (
    <FormProvider
      fallbackSubmitError="Something went wrong assigning the footnote"
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      <RHFForm id={id} onSubmit={handleSubmit}>
        <p>
          Select which subjects, filters and indicators your footnote applies to
          and these will appear alongside the associated data in your published
          statistics.
        </p>

        <p>
          Footnotes should be used sparingly, and only for information that is
          critical to understanding the data in the table or chart it refers to.
        </p>

        <RHFFormFieldEditor<BaseFootnote>
          name="content"
          label="Footnote"
          includePlugins={pluginsConfigLinksOnly}
          toolbarConfig={toolbarConfigLinkOnly}
        />

        {orderBy(
          Object.values(footnoteMeta.subjects),
          subject => subject.subjectName,
        ).map(subject => {
          const { subjectId, subjectName } = subject;

          return (
            <RHFFormFieldRadioGroup
              className={styles.radio}
              key={subjectId}
              testId={`footnote-subject ${subjectName}`}
              legend={
                <>
                  {`Subject: ${subjectName}`}
                  <VisuallyHidden>
                    {' '}
                    - select indicators and filters
                  </VisuallyHidden>
                </>
              }
              small
              inline
              showError={false}
              name={`subjects.${subjectId}.selectionType`}
              order={[]}
              options={[
                { value: 'NA', label: 'Does not apply' },
                {
                  value: `All`,
                  label: 'Applies to all data',
                },
                {
                  value: 'Specific',
                  label: 'Applies to specific data',
                  conditional: (
                    <div className="govuk-grid-row">
                      <div className="govuk-grid-column-one-half">
                        <h3 className="govuk-heading-s govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                          Indicators
                        </h3>

                        <IndicatorDetails
                          summary="Indicators"
                          valuePath={`subjects.${subjectId}`}
                          indicatorGroups={subject.indicators}
                        />
                      </div>

                      <div className="govuk-grid-column-one-half">
                        {Object.entries(subject.filters).length > 0 && (
                          <>
                            <h3 className="govuk-heading-s govuk-!-margin-bottom-2 govuk-!-margin-top-0">
                              Filters
                            </h3>

                            {Object.entries(subject.filters).map(
                              ([filterId, filter]) => (
                                <FilterGroupDetails
                                  key={filterId}
                                  summary={filter.legend}
                                  valuePath={`subjects.${subjectId}`}
                                  groupId={filterId}
                                  filter={filter}
                                  selectAll
                                />
                              ),
                            )}
                          </>
                        )}
                      </div>
                    </div>
                  ),
                },
              ]}
            />
          );
        })}

        <ButtonGroup>
          <Button type="submit">Save footnote</Button>
          {cancelButton}
        </ButtonGroup>
      </RHFForm>
    </FormProvider>
  );
}
