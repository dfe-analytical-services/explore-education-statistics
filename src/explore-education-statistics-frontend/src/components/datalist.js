import React, { Component } from 'react';
import PropTypes from 'prop-types'
import { Link } from 'react-router-dom'

class DataList extends Component {
    render() {
        return (
            <div>
                {this.props.data.length > 0 ? (
                    <ul>
                    {this.props.data.map(elem => (
                        <li key={elem.id}><Link to={`/${this.props.linkIdentifier}/${elem.id}`}>{elem.title}</Link></li>
                    ))}
                    </ul>
                ) : (
                    <p>None</p>
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