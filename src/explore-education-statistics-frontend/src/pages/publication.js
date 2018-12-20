import React, { Component } from 'react';
import PropTypes from 'prop-types'
import axios from 'axios';

class Publication extends Component {

    constructor(props) {
        super(props)
        this.state = {
            users: [],
            store: []
        }
    }

    componentDidMount() {
        const { handle } = this.props.match.params
        axios.get(`http://localhost:5010/api/Publication/${handle}`)
            .then(json => this.setState({ store: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const data = this.state.store;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <h1 className="govuk-heading-xl">Publication</h1>
                    <Pub key={data.id} label={data.title}></Pub>
                </div>
            </div>
        );
    }
}

class Pub extends Component {
    render() {
        return (
            <h2>{this.props.label}</h2>
        );
    }
}

Pub.propTypes = {
    label: PropTypes.string
}

Pub.defaultProps = {
    label: ''
}

export default Publication;