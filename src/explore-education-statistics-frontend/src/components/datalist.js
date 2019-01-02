import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { Link } from 'react-router-dom'

class DataList extends Component {
    render() {
        return (
            <div>
                {this.props.data.length > 0 ? (
                    <div className="govuk-grid-row">
                    {this.props.data.map(elem => (
                        <div className="govuk-grid-column-one-half" key={elem.id}>
                            <h4 className="govuk-heading-s">
                                <Link className="govuk-link" to={`${this.props.linkIdentifier}/${elem.id}`}>{elem.title}</Link>
                            </h4>
                            <p className="govuk-body">link description</p>
                        </div>
                    ))}
                    </div>
                ) : (
                    <div class="govuk-inset-text">
                        None currently published.
                    </div>
                )}
            </div>
        );
    }
}

DataList.propTypes = {
    linkIdentifier: PropTypes.string,
    data: PropTypes.array
}

DataList.defaultProps = {
    linkIdentifier: '',
    data: []
}

export default DataList;