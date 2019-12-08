/* eslint-disable react/destructuring-assignment,prefer-destructuring,no-param-reassign */

import EditableAccordion from '@admin/components/EditableAccordion';
import EditableAccordionSection from '@admin/components/EditableAccordionSection';
import Link from '@admin/components/Link';
import EditableContentBlock from '@admin/modules/find-statistics/components/EditableContentBlock';
import { EditableRelease } from '@admin/services/publicationService';
import RelatedAside from '@common/components/RelatedAside';
import { ContentBlockType } from '@common/services/publicationService';
import React, { Component } from 'react';
import {
  DragDropContext,
  Draggable,
  DraggableLocation,
  Droppable,
  DropResult,
} from 'react-beautiful-dnd';

interface State {
  reordering: boolean;
  data?: EditableRelease | undefined;
}

interface Props {
  editing?: boolean;
  higherReview?: boolean;
  reviewing?: boolean;
  resolveComments?: boolean;
  data: EditableRelease | undefined;
}

const ACCORDION_ID_REGEXP = /accordion[(]([0-9]+)[)]/;

function getAccordionIndex(id: string | null | undefined) {
  if (id) {
    const regexpResult = ACCORDION_ID_REGEXP.exec(id);
    if (regexpResult) return regexpResult[1];
  }
  return null;
}

class EditableMethodologyPage extends Component<Props, State> {
  public static defaultProps = { editing: false };

  public constructor(props: Props) {
    super(props);

    this.state = { reordering: false, data: undefined };
  }

  public componentDidMount() {
    const { data } = this.props;
    this.setState({ data });
  }

  public onDragEnd = (result: DropResult) => {
    if (result.destination) {
      const { source } = result;
      const target: DraggableLocation = result.destination;

      const { data } = this.state;

      if (result.type === 'accordion') {
        const sourceAccordion = getAccordionIndex(source.droppableId);
        const targetAccordion = getAccordionIndex(target.droppableId);

        if (sourceAccordion && targetAccordion) {
          if (source.index !== target.index) {
            if (data) {
              const release: EditableRelease = (data as unknown) as EditableRelease;

              const resultList = Array.from(release.content);

              const [moved] = resultList.splice(source.index, 1);
              resultList.splice(target.index, 0, moved);

              data.content = resultList;

              this.setState({ data: release });
            }
          }
        }
      }
    }
  };

  public addNewSection() {
    if (this.state.data) {
      const { data } = this.state;

      data.content.push({
        heading: 'New section',
        order: data.content.length + 1,
        caption: '',
        content: [
          {
            id: '0',
            type: 'MarkDownBlock' as ContentBlockType,
            body: 'editable',
            comments: [],
          },
        ],
      });

      this.setState({ data });
    }
  }

  private renderContentSections(
    data: EditableRelease,
    editing: boolean | undefined,
    reviewing: boolean | undefined,
    resolveComments: boolean | undefined,
  ) {
    const { reordering } = this.state;

    if (reordering) {
      return this.renderDraggableSections(data);
    }
    return this.renderEditableSections(
      data,
      editing,
      reviewing,
      resolveComments,
    );
  }

  private renderEditableSections(
    data: EditableRelease,
    editing: boolean | undefined,
    reviewing: boolean | undefined,
    resolveComments: boolean | undefined,
  ) {
    return (
      <div>
        <h2 className="govuk-heading-l reorderable-relative">
          <button
            className="govuk-button govuk-button--secondary reorderable"
            onClick={() => this.setState({ reordering: true })}
            type="button"
          >
            Reorder sections
          </button>
          Contents
        </h2>

        <EditableAccordion id="contents-sections">
          {data.content.map(({ heading, caption, order, content }, index) => (
            <EditableAccordionSection
              heading={`${order}. ${heading}`}
              caption={caption}
              index={index}
              key={`${order}_${heading}`}
            >
              <EditableContentBlock
                editable={editing}
                content={content}
                reviewing={reviewing}
                resolveComments={resolveComments}
                id={`editable-block-${index}`}
                publication={data.publication}
              />
            </EditableAccordionSection>
          ))}
        </EditableAccordion>

        <h2 className="govuk-heading-l reorderable-relative">
          <button
            type="button"
            className="govuk-button reorderable"
            onClick={() => this.addNewSection()}
          >
            Add new section
          </button>
        </h2>
      </div>
    );
  }

  private renderDraggableSections(data: EditableRelease) {
    return (
      <DragDropContext onDragEnd={this.onDragEnd}>
        <h2 className="govuk-heading-l reorderable-relative">
          <button
            className="govuk-button reorderable"
            onClick={() => this.setState({ reordering: false })}
            type="button"
          >
            Save reordering
          </button>
          Contents
        </h2>

        <div className="govuk-accordion__controls">&nbsp;</div>

        <Droppable droppableId="accordion(0)" type="accordion">
          {droppableProvided => (
            <div
              {...droppableProvided.droppableProps}
              ref={droppableProvided.innerRef}
            >
              {data &&
                data.content.map(({ heading, order }, index) => (
                  <Draggable
                    draggableId={`section(${order})`}
                    index={index}
                    key={`${order}`}
                  >
                    {draggableProvided => (
                      <div
                        className="govuk-accordion__section"
                        ref={draggableProvided.innerRef}
                        {...draggableProvided.draggableProps}
                      >
                        <div className="govuk-accordion__section-header">
                          <h2 className="govuk-accordion__section-heading reorderable-relative">
                            <span
                              className="drag-handle"
                              {...draggableProvided.dragHandleProps}
                            />
                            <button
                              className="govuk-accordion__section-button"
                              type="button"
                            >
                              {heading}
                            </button>
                          </h2>
                        </div>
                      </div>
                    )}
                  </Draggable>
                ))}

              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      </DragDropContext>
    );
  }

  public render() {
    const { reviewing } = this.props;
    const { higherReview } = this.props;
    const { resolveComments } = this.props;
    const { editing } = this.props;
    const { data } = this.state;

    return (
      <>
        <div className={editing ? 'page-editing' : ''}>
          <h1 className="govuk-heading-l">Example statistics: methodology</h1>

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-three-quarters">
                  <span className="govuk-tag">
                    {higherReview ? 'Ready for sign-off' : 'In draft'}
                  </span>

                  <dl className="dfe-meta-content">
                    <dt className="govuk-caption-m">Publish date: </dt>
                    <dd>
                      <strong>20 September 2019</strong>
                    </dd>
                  </dl>
                </div>
              </div>
            </div>

            <div className="govuk-grid-column-one-third">
              <RelatedAside>
                <nav role="navigation" aria-labelledby="related-content">
                  <h2 className="govuk-heading-m" id="related-content">
                    Related publications
                  </h2>

                  <ul className="govuk-list">
                    <li>
                      <Link to="/prototypes/methodology-absence">
                        This is an example of a related information link
                      </Link>
                    </li>
                  </ul>
                </nav>
              </RelatedAside>
            </div>
          </div>

          {/* {reviewing && data && (
            <AddComment initialComments={data.keyStatistics.comments} />
          )}
          {resolveComments && data && (
            <ResolveComment initialComments={data.keyStatistics.comments} />
          )} */}

          {data &&
            data.content.length > 0 &&
            this.renderContentSections(
              data,
              editing,
              reviewing,
              resolveComments,
            )}
          <h2 className="govuk-heading-l govuk-!-margin-top-9">Annexes</h2>
          {data &&
            data.content.length > 0 &&
            this.renderContentSections(
              data,
              editing,
              reviewing,
              resolveComments,
            )}
        </div>
      </>
    );
  }
}

export default EditableMethodologyPage;
