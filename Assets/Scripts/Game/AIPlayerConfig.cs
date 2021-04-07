using UnityEngine;

struct AiPlayerParams
{
    public float reactionTimeInSecond;
    public float decisionThreshold;
    public float speedOfMovement;
    public float maxMovementRadiusDistance;
    public float maxMovementHightRadiusDistance;
    public float movementProba;
    public float bodyMovementSpeed;
    public float handMovementSpeed;
    public float speedChange;
    public float rotationSpeed;
    public float grenadeForce;
}

static class AiPlayerConfig
{
    public static AiPlayerParams loadEasyConfig()
    {
        AiPlayerParams config;
        config.reactionTimeInSecond = 2.5f;
        config.decisionThreshold = 0.6f;
        config.speedOfMovement = 0.01f;
        config.maxMovementRadiusDistance = 1.0f;
        config.maxMovementHightRadiusDistance = 0.1f;
        config.movementProba = 0.3f;
        config.bodyMovementSpeed = 1.0f;
        config.handMovementSpeed = 1.0f;
        config.speedChange = 0.3f;
        config.rotationSpeed = 100.0f;
        config.grenadeForce = 1.0f;
        return config;
    }

    public static AiPlayerParams loadMediumConfig()
    {
        AiPlayerParams config;
        config.reactionTimeInSecond = 1.5f;
        config.decisionThreshold = 0.6f;
        config.speedOfMovement = 0.01f;
        config.maxMovementRadiusDistance = 1.2f;
        config.maxMovementHightRadiusDistance = 0.1f;
        config.movementProba = 0.3f;
        config.bodyMovementSpeed = 1.5f;
        config.handMovementSpeed = 1.5f;
        config.speedChange = 0.3f;
        config.rotationSpeed = 100.0f;
        config.grenadeForce = 1.0f;
        return config;
    }

    public static AiPlayerParams loadHardConfig()
    {
        AiPlayerParams config;
        config.reactionTimeInSecond = 0.8f;
        config.decisionThreshold = 0.6f;
        config.speedOfMovement = 0.01f;
        config.maxMovementRadiusDistance = 1.5f;
        config.maxMovementHightRadiusDistance = 0.1f;
        config.movementProba = 0.3f;
        config.bodyMovementSpeed = 2.0f;
        config.handMovementSpeed = 2.0f;
        config.speedChange = 0.3f;
        config.rotationSpeed = 100.0f;
        config.grenadeForce = 1.0f;
        return config;
    }
}