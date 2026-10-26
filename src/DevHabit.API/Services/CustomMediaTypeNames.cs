namespace DevHabit.API.Services;

public static class CustomMediaTypeNames
{
    public static class Application
    {
        public const string JsonV1 = "application/json;v=1";
        public const string JsonV2 = "application/json;v=2";
        // template for Api Versioning
        public const string HateoasJsonTemplate = "application/vnd.dev-habit.hateoas.{version}+json";
        public const string HateoasJson = "application/vnd.dev-habit.hateoas+json";
        public const string HateoasJsonV1 = "application/vnd.dev-habit.hateoas.1+json";
        public const string HateoasJsonV2 = "application/vnd.dev-habit.hateoas.2+json";
    }
}