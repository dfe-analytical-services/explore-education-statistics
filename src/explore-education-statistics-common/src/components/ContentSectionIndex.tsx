import React, { Component } from 'react';
import findAllForId from '@common/lib/dom/findAllForId';

interface IndexResult {
  element: Element;
  location: string;
}

interface Props {
  fromId: string;
}

interface State {
  indexResults: IndexResult[];
  indexComplete: boolean;
}

class ContentSectionIndex extends Component<Props> {
  public state: State = {
    indexResults: [],
    indexComplete: false,
  };

  public componentDidMount() {
    const { fromId } = this.props;
    this.index(fromId);
  }

  private static getLocationText(element: HTMLElement): string {
    const location: string[] = [];

    element.tagName.toLowerCase();
    location.unshift(element.textContent || '');
    return location.join(' > ');
  }

  private index = (fromId: string) => {
    const elements = findAllForId(fromId, 'h2,h3,h4');

    const indexResults =
      elements != null
        ? elements.map(element => {
            const location = ContentSectionIndex.getLocationText(element);

            return {
              element,
              location,
            };
          })
        : [];

    this.setState({ indexResults, indexComplete: true });
  };

  public render() {
    const { indexResults, indexComplete } = this.state;

    return (
      <>
        {indexComplete && <h3 className="govuk-heading-s">In this section</h3>}

        <ul className="govuk-body-s">
          {indexComplete &&
            indexResults.map((item, index) => {
              return (
                <>
                  {item.location && (
                    <li>
                      <a
                        href="#"
                        onClick={e => {
                          e.preventDefault();
                          indexResults[index].element.scrollIntoView();
                        }}
                      >
                        {item.location}
                      </a>
                    </li>
                  )}
                </>
              );
            })}
        </ul>
      </>
    );
  }
}

export default ContentSectionIndex;
